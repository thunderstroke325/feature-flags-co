using System.Linq;
using FeatureFlags.APIs.Authentication.Scheme;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers.Base
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ApiControllerBase : ControllerBase
    {
        public string CurrentUserId
        {
            get
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(p => p.Type == ApiClaims.UserId);
                return userIdClaim == null ? string.Empty : userIdClaim.Value;
            }
        }
    }
}