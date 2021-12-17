using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FeatureFlags.APIs.Services;
using FeatureFlags.Common.ExtensionMethods;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeatureFlags.APIs.Authentication.Scheme
{
    public class PublicApiAuthenticationHandler : AuthenticationHandler<PublicApiAuthenticationOptions>
    {
        private readonly IEnvironmentService _envService;

        public PublicApiAuthenticationHandler(
            IOptionsMonitor<PublicApiAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IEnvironmentService envService)
            : base(options, logger, encoder, clock)
        {
            _envService = envService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var envSecret = Request.EnvSecret();
            if (string.IsNullOrWhiteSpace(envSecret))
            {
                return AuthenticateResult.Fail("EnvSecret Not Found.");
            }

            var envId = await _envService.GetEnvIdBySecretAsync(envSecret);
            if (envId == 0)
            {
                return AuthenticateResult.Fail("Invalid EnvSecret Provided.");
            }

            // construct ticket
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "public-api-user-id")
            };
            var identity = new ClaimsIdentity(claims, PublicApiAuthenticationConstants.AuthenticationType);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}