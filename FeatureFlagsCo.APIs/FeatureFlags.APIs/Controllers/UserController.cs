using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.APIs.Controllers
{
    [ApiVersion("2.0")]
    public class UserController : ApiControllerBase
    {
        private readonly UserService _userService;
        private readonly AccountV2AppService _accountAppService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserService userService, 
            AccountV2AppService accountAppService, 
            ILogger<UserController> logger)
        {
            _logger = logger;
            _accountAppService = accountAppService;
            _userService = userService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("register-by-email")]
        public async Task<RegisterResult> RegisterByEmailAsync(RegisterByEmail request)
        {
            _logger.LogTrace("new user register by email {0}", request.Email);
            
            var registerResult = await _userService.RegisterByEmailAsync(request.Email, request.Password);
            if (registerResult.Success)
            {
                await _accountAppService.CreateAsync("Default Organization", registerResult.UserId, true);
            }
            
            return registerResult;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("send-identity-code")]
        public async Task<SendIdentityCodeResult> SendIdentityCodeAsync(
            [Required(AllowEmptyStrings = false)] string identity, 
            [Required(AllowEmptyStrings = false)] string scene)
        {
            _logger.LogTrace("send identity code to {0}", identity);
            
            var identityType = IdentityTypes.Check(identity);
            if (scene != "register")
            {
                var identityExists = await CheckIdentityExistsAsync(identity);
                if (!identityExists)
                {
                    _logger.LogWarning("identity {0} not exists but try send identity code", identity);
                    
                    var message = identityType == IdentityType.Email ? "该邮箱尚未注册" : "该手机号尚未注册";
                    return SendIdentityCodeResult.Failed(message);
                }
            }
            
            var result = await _userService.SendIdentityCode(identity, scene);
            return result;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("register-by-phone")]
        public async Task<RegisterResult> RegisterByPhoneAsync(RegisterByPhone request)
        {
            _logger.LogTrace("new user register by phone {0}", request.PhoneNumber);
            
            var registerResult = await _userService.RegisterByPhoneAsync(request.PhoneNumber, request.Code, request.Password);
            if (registerResult.Success)
            {
                await _accountAppService.CreateAsync("Default Organization", registerResult.UserId, true);
            }
            
            return registerResult;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("login-by-password")]
        public async Task<LoginResult> LoginByPasswordAsync(LoginByPassword request)
        {
            var identityType = IdentityTypes.Check(request.Identity);
            
            _logger.LogTrace("user {0} login in by password {1}", request.Identity, identityType);
            
            var loginResult = await _userService.LoginByPasswordAsync(request.Identity, request.Password);
            return loginResult;
        }
        
        [HttpPost]
        [AllowAnonymous]
        [Route("login-by-phone-code")]
        public async Task<LoginResult> LoginByPhoneCodeAsync(LoginByPhoneCode request)
        {
            _logger.LogTrace("user {0} login in by phoneNumber", request.PhoneNumber);
            
            var loginResult = await _userService.LoginByPhoneCodeAsync(request.PhoneNumber, request.Code);
            return loginResult;
        }
        
        [HttpGet]
        [AllowAnonymous]
        [Route("check-identity-exists")]
        public async Task<bool> CheckIdentityExistsAsync(string identity)
        {
            var exists = await _userService.CheckIdentityExistsAsync(identity);

            return exists;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("reset-password")]
        public async Task<ResetResult> ResetPasswordAsync(Reset request)
        {
            _logger.LogTrace("user {0} reset password", request.Identity);
            
            var result = await _userService.ResetPasswordAsync(request.Identity, request.Code, request.NewPassword);
            return result;
        }

        [HttpGet]
        [Route("user-profile")]
        public async Task<object> GetUserInfoAsync()
        {
            var user = await _userService.FindOneAsync(x => x.Id == CurrentUserId);

            var info = new
            {
                user.Id, 
                user.Email,
                user.UserName,
                user.PhoneNumber
            };

            return info;
        }

        [HttpPut]
        [Route("user-profile")]
        public IActionResult UpdateProfileAsync(UpdateProfile profile)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
    }
}