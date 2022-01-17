using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeatureFlags.APIs.Authentication.Scheme
{
    public class PublicApiAuthenticationHandler : AuthenticationHandler<PublicApiAuthenticationOptions>
    {
        public PublicApiAuthenticationHandler(
            IOptionsMonitor<PublicApiAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var envSecret = Request.EnvSecret();
            if (string.IsNullOrWhiteSpace(envSecret))
            {
                return Task.FromResult(AuthenticateResult.Fail("EnvSecret Not Found."));
            }

            var secret = EnvironmentSecretV2.Parse(envSecret);
            if (secret.EnvId == 0)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid EnvSecret Provided."));
            }

            // construct ticket
            var claims = new[]
            {
                new Claim(PublicApiClaims.EnvId, secret.EnvId.ToString())
            };
            var identity = new ClaimsIdentity(claims, PublicApiAuthenticationConstants.AuthenticationType);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}