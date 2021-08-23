using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using FeatureFlagsCo.MQ;
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

        public VariationController(
            ILogger<VariationController> logger, 
            IDistributedCache redisCache,
            IVariationService variationService,
            IOptions<MySettings> mySettings,
            IInsighstMqService insightsService)
        {
            _logger = logger;
            _redisCache = redisCache;
            _variationService = variationService;
            _mySettings = mySettings;
            _insightsService = insightsService;
        }


        #region old version of true/false status variation
        [HttpPost]
        [Route("GetUserVariationResult")]
        public async Task<bool?> GetVariation([FromBody] GetUserVariationResultParam param)
        {
            Tuple<bool?, bool> returnResult = await GetVariationCore(param);

            return returnResult.Item1;
        }


        private async Task<Tuple<bool?, bool>> GetVariationCore(GetUserVariationResultParam param)
        {
            var ffIdVM = FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(param.EnvironmentSecret, param.FeatureFlagKeyName);
            var returnResult = await _variationService.TrueFalseStatusCheckVariableAsync(param.EnvironmentSecret, param.FeatureFlagKeyName,
                new EnvironmentUser()
                {
                    Country = param.FFUserCountry,
                    CustomizedProperties = param.FFUserCustomizedProperties,
                    Email = param.FFUserEmail,
                    KeyId = param.FFUserKeyId,
                    Name = param.FFUserName
                },
                ffIdVM);
            var customizedTraceProperties = new Dictionary<string, object>()
            {
                ["envId"] = ffIdVM.EnvId,
                ["accountId"] = ffIdVM.AccountId,
                ["projectId"] = ffIdVM.ProjectId,
                ["featureFlagKey"] = param.FeatureFlagKeyName,
                ["userKey"] = param.FFUserKeyId,
                ["readOnlyOperation"] = returnResult.Item2,
            };
            using (_logger.BeginScope(customizedTraceProperties))
            {
                _logger.LogInformation("variation-request");
            }

            return returnResult;
        }

        [HttpPost]
        [Route("GetUserVariationResultInJson")]
        public async Task<GetUserVariationResultJsonViewModel> GetVariationInJson([FromBody] GetUserVariationResultParam param)
        {
            Tuple<bool?, bool> returnResult = await GetVariationCore(param);

            return new GetUserVariationResultJsonViewModel()
            {
                VariationResult = returnResult.Item1
            };
        }
        #endregion

        [HttpPost]
        [Route("GetMultiOptionVariation")]
        public async Task<JsonResult> GetMultiOptionVariation([FromBody] GetUserVariationResultParam param)
        {
            if(param == null)
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


                if(_mySettings.Value.HostingType == HostingTypeEnum.Local.ToString())
                {
                    SendToRabbitMqThenGrafana(param, ffIdVM, returnResult);
                }
                else if (_mySettings.Value.HostingType == HostingTypeEnum.Azure.ToString())
                {
                    SendToApplicationInsights(param, ffIdVM, returnResult);
                }

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

        private void SendToRabbitMqThenGrafana(GetUserVariationResultParam param, FeatureFlagIdByEnvironmentKeyViewModel ffIdVM, Tuple<VariationOption, bool> returnResult)
        {
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
            _insightsService.SendMessage(new FeatureFlagsCo.MQ.MessageModel
            {
                SendDateTime = DateTime.UtcNow,
                Labels = labels,
                Message = JsonConvert.SerializeObject(param ?? new GetUserVariationResultParam())
            });
        }
    }
}
