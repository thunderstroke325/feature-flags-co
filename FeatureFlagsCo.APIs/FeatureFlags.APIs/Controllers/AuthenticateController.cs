using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication.Scheme;

namespace FeatureFlags.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IOptions<MySettings> _mySettings;
        private readonly IUserInvitationService _userInvitationService;
        private readonly ILogger<AuthenticateController> _logger;

        public AuthenticateController(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IOptions<MySettings> mySettings,
            IUserInvitationService userInvitationService,
            ILogger<AuthenticateController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _mySettings = mySettings;
            _userInvitationService = userInvitationService;
            _logger = logger;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody]LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ApiClaims.UserId, user.Id)
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddMonths(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(
            [FromServices] AccountV2AppService accountAppService,
            [FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "User already exists!" });

            string firstCode = "ACEGIKMOQSUWY", thirdCode = "13579";
            if (string.IsNullOrWhiteSpace(model.InviteCode) || model.InviteCode.Length != 4 || 
                !firstCode.Contains(model.InviteCode[0]) || !thirdCode.Contains(model.InviteCode[2]))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "邀请码不正确" });
            }

            var result = await _userManager.CreateAsync(
                new ApplicationUser()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Email,
                    EmailConfirmed = true,
                    PhoneNumber = model.PhoneNumber
                }, 
                model.Password
            );

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = result.Errors.ToList().First().Description });

            var user = await _userManager.FindByEmailAsync(model.Email);

            // create account for new user
            var account = await accountAppService.CreateAsync(model.OrgName, user.Id, true);

            _logger.LogTrace($"{model.OrgName}的{model.Email}使用{model.InviteCode}注册了账户");

            return Ok(new Response { Code = "Success", Message = "User created successfully!" });
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "User already exists!" });

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

            return Ok(new Response { Code = "Success", Message = "User created successfully!" });
        }

        [HttpPost]
        [Route("forgetpassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

            string url = "http://api.sendcloud.net/apiv2/mail/sendtemplate";
            HttpClient client = new HttpClient();
            var resetUrl = _mySettings.Value.AdminWebPortalUrl + "/login/resetpassword?tokenid=" + token;
            string xsmtpapi = "{\"to\": [\""+ model.Email + "\"], \"sub\" : { \"%token%\" : [\"" + resetUrl + "\"]}}";
            List<KeyValuePair<String, String>> paramList = new List<KeyValuePair<String, String>>();
            paramList.Add(new KeyValuePair<string, string>("apiUser", _mySettings.Value.SendCloudAPIUser));
            paramList.Add(new KeyValuePair<string, string>("apiKey", _mySettings.Value.SendCloudAPIKey));
            paramList.Add(new KeyValuePair<string, string>("from", _mySettings.Value.SendCloudFrom));
            paramList.Add(new KeyValuePair<string, string>("fromName", _mySettings.Value.SendCloudFromName));
            paramList.Add(new KeyValuePair<string, string>("xsmtpapi", xsmtpapi));
            paramList.Add(new KeyValuePair<string, string>("subject", _mySettings.Value.SendCloudEmailSubject + "找回密码"));
            paramList.Add(new KeyValuePair<string, string>("templateInvokeName", _mySettings.Value.SendCloudTemplate));

            HttpResponseMessage response = client.PostAsync(url, new FormUrlEncodedContent(paramList)).Result;
            if(response.StatusCode.ToString() == "OK")
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordModel resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user == null)
                return NotFound();

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                    ModelState.AddModelError(error.Code, error.Description);
                return BadRequest(ModelState);
            }

            return Ok();
        }



        [HttpPost]
        [Authorize]
        [Route("logout")]
        public IActionResult Logout()
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            return Ok();
        }


        [HttpGet]
        [Route("Probe")]
        public string Probe()
        {
            return "Good";
        }

        [HttpGet]
        [Authorize]
        [Route("MyInfo")]
        public async Task<dynamic> MyInfo()
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            var userExists = await _userManager.FindByIdAsync(currentUserId);
            if (userExists == null)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Code = "Error", Message = "User not found!" });
            return new
            {
                Email = userExists.Email,
                PhoneNumber = userExists.PhoneNumber
            };
        }

        [HttpPost]
        [Authorize]
        [Route("updateinfo")]
        public async Task<IActionResult> UpdateInfo([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists == null)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Code = "Error", Message = "User not found!" });

            userExists.PhoneNumber = model.PhoneNumber;
            var token = await _userManager.GeneratePasswordResetTokenAsync(userExists);
            var result = await _userManager.UpdateAsync(userExists);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = result.Errors.ToList().First().Description });
            result = await _userManager.ResetPasswordAsync(userExists, token, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = result.Errors.ToList().First().Description });

            // remove all invitations to the user
            await _userInvitationService.ClearAsync(userExists.Id);

            return Ok(new Response { Code = "Success", Message = "User info updated successfully!" });
        }

    }
}
