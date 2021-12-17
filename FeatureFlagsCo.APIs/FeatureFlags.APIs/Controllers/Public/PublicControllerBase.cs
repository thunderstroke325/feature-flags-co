using FeatureFlags.APIs.Authentication.Scheme;
using FeatureFlags.Common.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers.Public
{
    /// <summary>
    /// controller base for public apis
    /// </summary>
    [Authorize(AuthenticationSchemes = PublicApiAuthenticationConstants.Scheme)]
    [ApiController]
    [Route("api/public/")]
    public class PublicControllerBase : ControllerBase
    {
        protected string EnvSecret => HttpContext.Request.EnvSecret();
    }
}