using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Authing.ApiClient.Types;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Authentication.Scheme;
using FeatureFlags.APIs.Services.Authing;
using FeatureFlags.APIs.ViewModels.User;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FeatureFlags.APIs.Services
{
    public class UserService : ITransientDependency
    {
        private readonly JWTSettings _jwtOptions;
        private readonly ApplicationDbContext _sqlserver;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthingService _authingService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IOptions<JWTSettings> jwtOptions, 
            ApplicationDbContext sqlserver, 
            UserManager<ApplicationUser> userManager, 
            AuthingService authingService, 
            ILogger<UserService> logger)
        {
            _jwtOptions = jwtOptions.Value;
            _sqlserver = sqlserver;
            _userManager = userManager;
            _authingService = authingService;
            _logger = logger;
        }
        
        public async Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser> FindOneAsync(Expression<Func<ApplicationUser, bool>> predicate)
        {
            var user = await _sqlserver.Users.FirstOrDefaultAsync(predicate);

            return user;
        }

        public async Task<RegisterResult> RegisterByEmailAsync(string email, string password)
        {
            // create authing user
            var response = await _authingService.RegisterByEmailAsync(email, password);
            if (!response.Success)
            {
                var message = $"register authing user by email {email} failed, reason: {response.Message}";
                var ex = new UserRegistrationException(message);
                _logger.LogError(ex, ex.Message);
                
                return RegisterResult.Failed(response.Message);
            }

            // register app user
            var registerAppUserResult = await RegisterAppUserAsync(response.Data, password);
            return registerAppUserResult;
        }

        public async Task<bool> SendIdentityCode(string identity, string scene)
        {
            var identityType = IdentityTypes.Check(identity);

            AuthingResponse<CommonMessage> response;
            switch (identityType)
            {
                case IdentityType.Email when scene == "forget-password":
                    response = await _authingService.SendEmailCodeAsync(identity, EmailScene.RESET_PASSWORD);
                    break;
                case IdentityType.Phone:
                    response = await _authingService.SendPhoneCodeAsync(identity);
                    break;

                default:
                    var ex = new NotSupportedException($"identity type({identity}) with scene({scene}) not supported");
                    _logger.LogError(ex, ex.Message);
                    return false;
            }

            if (!response.Success || response.Data.Code != 200)
            {
                var reason = response.Data != null ? response.Data.Message : response.Message;
                var ex = new SendIdentityCodeException($"send identity code failed, reason: {reason}");
                _logger.LogError(ex, ex.Message);
                return false;
            }

            return true;
        }

        public async Task<RegisterResult> RegisterByPhoneAsync(string phoneNumber, string code, string password)
        {
            // create authing user
            var response = await _authingService.RegisterByPhoneAsync(phoneNumber, code, password);
            if (!response.Success)
            {
                return RegisterResult.Failed(response.Message);
            }
            
            // register app user
            var registerAppUserResult = await RegisterAppUserAsync(response.Data, password);
            return registerAppUserResult;
        }

        public async Task<LoginResult> LoginByPasswordAsync(string identity, string password)
        {
            var identityType = IdentityTypes.Check(identity);
            
            if (identityType == IdentityType.Email)
            {
                var emailLoginResult = await LoginByEmailAsync(identity, password);
                return emailLoginResult;
            }

            if (identityType == IdentityType.Phone)
            {
                var phoneLoginResult = await LoginByPhonePasswordAsync(identity, password);
                return phoneLoginResult;
            }
            
            return LoginResult.Failed("identity type not supported");
        }

        public async Task<LoginResult> LoginByPhoneCodeAsync(string phoneNumber, string code)
        {
            var response = await _authingService.LoginByPhoneCodeAsync(phoneNumber, code);
            if (!response.Success)
            {
                return LoginResult.Failed(response.Message);
            }

            var token = IssuingToken(response.Data);
            return LoginResult.Ok(token);
        }

        public async Task<LoginResult> LoginByEmailAsync(string email, string password)
        {
            var (authingLoginResult, code) = await AuthingEmailLoginAsync(email, password);
            if (authingLoginResult.Success)
            {
                return authingLoginResult;
            }

            // 邮件未验证
            if (code == AuthingErrors.EmailNotVerified)
            {
                return authingLoginResult;
            }

            // asp.net core identity login (for old user)
            var identityLoginResult = await AspNetCoreIdentityEmailLoginAsync(email, password);
            return identityLoginResult;
        }

        public async Task<LoginResult> LoginByPhonePasswordAsync(string phone, string password)
        {
            var response = await _authingService.LoginByPhonePasswordAsync(phone, password);
            if (!response.Success)
            {
                return LoginResult.Failed(response.Message);
            }
            
            var token = IssuingToken(response.Data);
            return LoginResult.Ok(token);
        }

        /// <summary>
        /// authing login by email
        /// </summary>
        /// <returns></returns>
        public async Task<(LoginResult result, int code)> AuthingEmailLoginAsync(string email, string password)
        {
            var response = await _authingService.LoginByEmailAsync(email, password);
            if (!response.Success)
            {
                return (LoginResult.Failed(response.Message), response.Code);
            }

            var token = IssuingToken(response.Data);
            return (LoginResult.Ok(token), response.Code);
        }
        
        /// <summary>
        /// asp.net core identity login by email (for old user)
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<LoginResult> AspNetCoreIdentityEmailLoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return LoginResult.Failed("该邮箱尚未注册");
            }
            
            var passwordMatch = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordMatch)
            {
                return LoginResult.Failed("邮箱/密码不匹配");
            }

            var token = IssuingToken(user);
            return LoginResult.Ok(token);
        }

        public async Task<ResetResult> ResetPasswordAsync(string identity, string code, string newPassword)
        {
            var authingResetResult = await AuthingResetPasswordAsync(identity, code, newPassword);
            if (!authingResetResult.Success)
            {
                return authingResetResult;
            }
            
            var appUserResetResult = await AspNetIdentityResetPasswordAsync(identity, newPassword);
            return appUserResetResult;
        }

        public async Task<ResetResult> AuthingResetPasswordAsync(
            string identity, 
            string code, 
            string newPassword)
        {
            var response = await _authingService.ResetPasswordAsync(identity, code, newPassword);
            
            return response.Success 
                ? ResetResult.Ok() 
                : ResetResult.Failed(response.Message);
        }

        public async Task<ResetResult> AspNetIdentityResetPasswordAsync(
            string identity,
            string newPassword)
        {
            var identityType = IdentityTypes.Check(identity);

            var user = identityType == IdentityType.Email
                ? await _sqlserver.Users.FirstOrDefaultAsync(x => x.Email == identity)
                : await _sqlserver.Users.FirstOrDefaultAsync(x => x.PhoneNumber == identity);
            if (user == null)
            {
                return ResetResult.Failed("该用户不存在");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var identityResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            
            return identityResult.Succeeded
                ? ResetResult.Ok()
                : ResetResult.Failed(identityResult.ToString());
        }

        public async Task<bool> CheckIdentityExistsAsync(string identity)
        {
            var identityType = IdentityTypes.Check(identity);

            var user = identityType == IdentityType.Email
                ? await _sqlserver.Users.FirstOrDefaultAsync(x => x.Email == identity)
                : await _sqlserver.Users.FirstOrDefaultAsync(x => x.PhoneNumber == identity);

            return user != null;
        }

        public string IssuingToken(User user)
        {
            var appUser = new ApplicationUser(user);

            var token = IssuingToken(appUser);
            return token;
        }

        public string IssuingToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ApiClaims.UserId, user.Id), 
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                _jwtOptions.ValidIssuer,
                _jwtOptions.ValidAudience,
                expires: DateTime.Now.AddMonths(1),
                claims: claims,
                signingCredentials: credentials
            );
            
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(jwt);
        }
        
        public async Task<RegisterResult> RegisterAppUserAsync(User authingUser, string password)
        {
            var appUser = new ApplicationUser(authingUser);
            
            var createAppUserResult = await _userManager.CreateAsync(appUser, password);
            if (!createAppUserResult.Succeeded)
            {
                var message = $"create app user({appUser.UserName}) by authingUser failed, reason {createAppUserResult}";
                var ex = new UserRegistrationException(message);
                _logger.LogError(ex, ex.Message);

                return RegisterResult.Failed(ex.Message);
            }

            return RegisterResult.Ok(appUser.Id);
        }
    }
}