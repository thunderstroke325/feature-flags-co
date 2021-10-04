using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class VariationController : ControllerBase
    {
        private readonly ILogger<VariationController> _logger;
        private readonly IDistributedCache _redisCache;
        private readonly IVariationService _variationService;
        private readonly IOptions<MySettings> _mySettings;
        private readonly IInsighstMqService _insightsService;
        private readonly IFeatureFlagMqService _featureFlagMqService;

        public VariationController(
            ILogger<VariationController> logger, 
            IDistributedCache redisCache,
            IVariationService variationService,
            IOptions<MySettings> mySettings,
            IInsighstMqService insightsService,
            IFeatureFlagMqService featureFlagMqService)
        {
            _logger = logger;
            _redisCache = redisCache;
            _variationService = variationService;
            _mySettings = mySettings;
            _insightsService = insightsService;
            _featureFlagMqService = featureFlagMqService;
        }

        [HttpPost]
        [Route("GetMultiOptionVariation")]
        public async Task<dynamic> GetMultiOptionVariation([FromBody] GetUserVariationResultParam param)
        {

            if (param == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return new JsonResult("Parameter incorrect");
            }
            else if (string.IsNullOrWhiteSpace(param.EnvironmentSecret))
            {
                Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return new JsonResult("EnvironmentSecret shouldn't be empty");
            }
            else if (string.IsNullOrWhiteSpace(param.FeatureFlagKeyName))
            {
                Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return new JsonResult("FeatureFlagKeyName shouldn't be empty");
            }
            else if (string.IsNullOrWhiteSpace(param.FFUserKeyId))
            {
                Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return new JsonResult("FFUserKeyId shouldn't be empty");
            }
            try
            {
                var ffIdVM = FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(param.EnvironmentSecret, param.FeatureFlagKeyName);
                var returnResult = await _variationService.CheckMultiOptionVariationAsync(param.EnvironmentSecret, param.FeatureFlagKeyName,
                    new EnvironmentUser()
                    {
                        Country = param.FFUserCountry,
                        CustomizedProperties = param.FFUserCustomizedProperties,
                        Email = param.FFUserEmail,
                        KeyId = param.FFUserKeyId,
                        Name = param.FFUserName
                    },
                    ffIdVM);

                if (returnResult == null) 
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Feature Flag doesn't exist, please verify your featureFlagKeyName" });
                }

                SendToRabbitMQ(param, ffIdVM, returnResult);

                return new JsonResult(returnResult.Item1);
            }
            catch(Exception exp)
            {
                _logger.LogError(exp, "Post /Variation/GetMultiOptionVariation ; Body: " + JsonConvert.SerializeObject(param));
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return new JsonResult(exp.Message);
            }
        }

        private void SendToApplicationInsights(GetUserVariationResultParam param, FeatureFlagIdByEnvironmentKeyViewModel ffIdVM, Tuple<VariationOption, bool> returnResult)
        {
            var customizedTraceProperties = new Dictionary<string, object>()
            {
                ["envId"] = ffIdVM.EnvId,
                ["accountId"] = ffIdVM.AccountId,
                ["projectId"] = ffIdVM.ProjectId,
                ["featureFlagKey"] = param.FeatureFlagKeyName,
                ["userKey"] = param.FFUserKeyId,
                ["readOnlyOperation"] = returnResult.Item2,
                ["variationValue"] = returnResult.Item1.VariationValue,
                ["featureFlagId"] = ffIdVM.FeatureFlagId
            };
            using (_logger.BeginScope(customizedTraceProperties))
            {
                _logger.LogInformation("variation-request");
            }
        }

        private void SendToRabbitMQ(GetUserVariationResultParam param, FeatureFlagIdByEnvironmentKeyViewModel ffIdVM, Tuple<VariationOption, bool> returnResult)
        {
            var ffEvent = new FeatureFlagMessageModel()
            {
                RequestPath = "/Variation/GetMultiOptionVariation",
                FeatureFlagId = ffIdVM.FeatureFlagId,
                EnvId = ffIdVM.EnvId,
                AccountId = ffIdVM.AccountId,
                ProjectId = ffIdVM.ProjectId,
                FeatureFlagKeyName = param.FeatureFlagKeyName,
                UserKeyId = param.FFUserKeyId,
                FFUserName = param.FFUserName,
                VariationLocalId = returnResult.Item1.LocalId.ToString(),
                VariationValue = returnResult.Item1.VariationValue,
                TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff")
            };
            
            var labels = new List<FeatureFlagsCo.MQ.MessageLabel>()
                         {
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "RequestPath",
                                  LabelValue = "/Variation/GetMultiOptionVariation"
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "FeatureFlagId",
                                  LabelValue = ffIdVM.FeatureFlagId
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "EnvId",
                                  LabelValue = ffIdVM.EnvId
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "AccountId",
                                  LabelValue = ffIdVM.AccountId
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "ProjectId",
                                  LabelValue = ffIdVM.ProjectId
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "FeatureFlagKeyName",
                                  LabelValue = param.FeatureFlagKeyName
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "UserKeyId",
                                  LabelValue = param.FFUserKeyId
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "FFUserName",
                                  LabelValue = param.FFUserName
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "VariationLocalId",
                                  LabelValue = returnResult.Item1.LocalId.ToString()
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "VariationValue",
                                  LabelValue = returnResult.Item1.VariationValue
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "TimeStamp",
                                  LabelValue = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff")
                              } 
                        };
            if(param.FFUserCustomizedProperties != null && param.FFUserCustomizedProperties.Count > 0)
            {
                foreach (var item in param.FFUserCustomizedProperties)
                {
                    labels.Add(new FeatureFlagsCo.MQ.MessageLabel
                    {
                        LabelName = item.Name,
                        LabelValue = item.Value
                    });
                }
            }
            _featureFlagMqService.SendMessage(ffEvent);
            _insightsService.SendMessage(new FeatureFlagsCo.MQ.MessageModel
            {
                SendDateTime = DateTime.UtcNow,
                Labels = labels,
                Message = JsonConvert.SerializeObject(param ?? new GetUserVariationResultParam()),
                FeatureFlagId = ffIdVM.FeatureFlagId,
                IndexTarget = "ffvariationrequestindex"
            });
        }
    }
}
