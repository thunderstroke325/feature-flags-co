using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Experiments;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ExperimentsDataReceiverController : ControllerBase
    {
        private readonly ILogger<ExperimentsDataReceiverController> _logger;
        private readonly MessagingService _messagingService;

        public ExperimentsDataReceiverController(
            ILogger<ExperimentsDataReceiverController> logger,
            MessagingService messagingService)
        {
            _logger = logger;
            _messagingService = messagingService;
        }


        [HttpPost]
        [Route("PushData")]
        public dynamic PushData([FromBody] List<ExperimentMessageViewModel> param)
        {
            if (param != null && param.Count > 0)
            {
                foreach (var item in param)
                {
                    var ffIdVm = FeatureFlagKeyExtension.GetEnvIdsByEnvKey(item.Secret);
                    var message = new ExperimentMessageModel 
                    {
                        Route = item.Route,
                        Secret = item.Secret,
                        Type = item.Type,
                        EventName = item.EventName,
                        NumericValue = item.NumericValue,
                        User = item.User,
                        AppType = item.AppType,
                        CustomizedProperties = item.CustomizedProperties,
                        ProjectId = ffIdVm.ProjectId,
                        EnvironmentId = ffIdVm.EnvId,
                        AccountId = ffIdVm.AccountId,
                        TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff")
                    };

                    _messagingService.SendEventDataWithoutResponse(message);
                }
                return new JsonResult(null);
            }
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return new JsonResult(null);
        }
    }
}
