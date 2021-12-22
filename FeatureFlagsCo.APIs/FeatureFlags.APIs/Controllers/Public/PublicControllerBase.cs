using System;
using System.Linq;
using FeatureFlags.APIs.Authentication.Scheme;
using FeatureFlags.APIs.MvcFilters;
using FeatureFlags.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers.Public
{
    /// <summary>
    /// controller base for public apis
    /// </summary>
    [Authorize(AuthenticationSchemes = PublicApiAuthenticationConstants.Scheme)]
    [ApiController]
    [ApiActionFilter]
    [Route("api/public/")]
    public class PublicControllerBase : ControllerBase
    {
        protected string EnvSecret => Request.EnvSecret();
        
        protected int EnvId
        {
            get
            {
                var envIdStr = User.Claims.First(claim => claim.Type == PublicApiClaims.EnvId).Value;
                return Convert.ToInt32(envIdStr);
            }
        }
    }
}