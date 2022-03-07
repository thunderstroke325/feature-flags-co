using System;
using System.Threading.Tasks;
using Authing.ApiClient;
using Authing.ApiClient.Auth;
using Authing.ApiClient.Auth.Types;
using Authing.ApiClient.Types;
using FeatureFlags.APIs.Authentication;
using Microsoft.AspNetCore.Http;
using FeatureFlags.Utils.ConventionalDependencyInjection;

namespace FeatureFlags.APIs.Services.Authing
{
    public class AuthingResponse<TData>
    {
        public bool Success { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }

        public TData Data { get; set; }
    }

    public class AuthingService : IScopedDependency
    {
        private readonly AuthenticationClient _authingClient;
        private readonly RegisterAndLoginOptions _registerAndLoginOptions;

        public AuthingService(
            IHttpContextAccessor httpContextAccessor,
            AuthenticationClient authingClient)
        {
            _authingClient = authingClient;

            var httpContext = httpContextAccessor.HttpContext;
            _registerAndLoginOptions = new RegisterAndLoginOptions
            {
                ClientIp = httpContext.Connection.RemoteIpAddress?.ToString()
            };
        }

        public async Task<AuthingResponse<User>> RegisterByEmailAsync(string email, string password)
        {
            var request = _authingClient.RegisterByEmail(email, password, null, _registerAndLoginOptions);

            var response = await HandleRequestAsync(() => request);
            return response;
        }

        public async Task<AuthingResponse<CommonMessage>> SendPhoneCodeAsync(string phone)
        {
            var request = _authingClient.SendSmsCode(phone);

            var response = await HandleRequestAsync(() => request);
            return response;
        }

        public async Task<AuthingResponse<CommonMessage>> SendEmailCodeAsync(string email, EmailScene scene)
        {
            var request = _authingClient.SendEmail(email, scene);

            var response = await HandleRequestAsync(() => request);
            return response;
        }

        public async Task<AuthingResponse<User>> RegisterByPhoneAsync(string phone, string code, string password)
        {
            var request =
                _authingClient.RegisterByPhoneCode(phone, code, password, null, _registerAndLoginOptions);

            var response = await HandleRequestAsync(() => request);
            return response;
        }

        public async Task<AuthingResponse<User>> LoginByEmailAsync(string email, string password)
        {
            var request = _authingClient.LoginByEmail(email, password, _registerAndLoginOptions);

            var response = await HandleRequestAsync(() => request);
            return response;
        }

        public async Task<AuthingResponse<User>> LoginByPhoneCodeAsync(string phone, string code)
        {
            var request = _authingClient.LoginByPhoneCode(phone, code, _registerAndLoginOptions);

            var response = await HandleRequestAsync(() => request);
            return response;
        }

        public async Task<AuthingResponse<User>> LoginByPhonePasswordAsync(string phone, string password)
        {
            var request = _authingClient.LoginByPhonePassword(phone, password, _registerAndLoginOptions);

            var response = await HandleRequestAsync(() => request);
            return response;
        }

        public async Task<AuthingResponse<CommonMessage>> ResetPasswordAsync(
            string identity,
            string code,
            string newPassword)
        {
            var identityType = IdentityTypes.Check(identity);

            var request = identityType == IdentityType.Email
                ? _authingClient.ResetPasswordByEmailCode(identity, code, newPassword)
                : _authingClient.ResetPasswordByPhoneCode(identity, code, newPassword);

            var response = await HandleRequestAsync(() => request);
            return response;
        }

        public async Task<AuthingResponse<User>> UpdateProfileAsync(string id, string name)
        {
            var updates = new UpdateUserInput
            {
                Username = name
            };

            var request = _authingClient.UpdateProfile(updates);
            
            var response = await HandleRequestAsync(() => request);
            return response;
        }

        private async Task<AuthingResponse<TData>> HandleRequestAsync<TData>(Func<Task<TData>> request)
        {
            try
            {
                var data = await request();

                var response = new AuthingResponse<TData>
                {
                    Success = true,
                    Code = 200,
                    Message = "authing request success",
                    Data = data
                };

                return response;
            }
            catch (AuthingException ex)
            {
                var response = new AuthingResponse<TData>
                {
                    Success = false,
                    Code = ex.StatusCode,
                    Message = AuthingErrors.Describe(ex.Message),
                    Data = default
                };

                return response;
            }
        }
    }
}