using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VariationController : ControllerBase
    {
        private readonly ILogger<VariationController> _logger;
        private readonly IVariationService _variationService;
        private readonly MessagingService _messagingService;
        private readonly MongoDbFeatureFlagService _mongoDbFeatureFlagService;

        public VariationController(
            ILogger<VariationController> logger, 
            IVariationService variationService,
            MongoDbFeatureFlagService mongoDbFeatureFlagService,
            MessagingService messagingService)
        {
            _logger = logger;
            _variationService = variationService;
            _messagingService = messagingService;
            _mongoDbFeatureFlagService = mongoDbFeatureFlagService;
        }

        [HttpPost]
        [Route("SendUserVariation")]
        public async Task<dynamic> SendUserVariation([FromBody] SendUserVariationViewModel param) 
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
                var ffs = await _mongoDbFeatureFlagService.GetActiveByIdsAsync(new List<string> { ffIdVM.FeatureFlagId });
                VariationOption variation = null;

                if (ffs != null && ffs.Count == 1)
                {
                    var ff = ffs[0];
                    variation = ff.VariationOptions.Find(v => v.LocalId.Equals(param.VariationOptionId));
                }

                if (variation == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Feature Flag doesn't exist, please verify your featureFlagKeyName" });
                }

                try
                {
                    SendToRabbitMQ(param, ffIdVM, new CachedUserVariation(variation, true));
                }
                catch (Exception exp)
                {
                    _logger.LogError(exp, "Post /Variation/GetMultiOptionVariation:SendToRabbitMQ ; Body: " + JsonConvert.SerializeObject(param));
                }

                return new JsonResult(variation.VariationValue);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Post /Variation/SendUserVariation ; Body: " + JsonConvert.SerializeObject(param));
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return new JsonResult(exp.Message);
            }
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
                var ffIdVm = FeatureFlagKeyExtension.GetFeatureFlagIdByEnvironmentKey(param.EnvironmentSecret, param.FeatureFlagKeyName);
                var userVariation = await _variationService.GetUserVariationAsync(param.EnvironmentSecret,
                    new EnvironmentUser()
                    {
                        Country = param.FFUserCountry,
                        CustomizedProperties = param.FFUserCustomizedProperties,
                        Email = param.FFUserEmail,
                        KeyId = param.FFUserKeyId,
                        Name = param.FFUserName
                    },
                    ffIdVm);

                if (userVariation == null) 
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = "Feature Flag doesn't exist, please verify your featureFlagKeyName" });
                }

                try
                {
                    SendToRabbitMQ(param, ffIdVm, userVariation);
                }
                catch(Exception exp)
                {
                    _logger.LogError(exp, "Post /Variation/GetMultiOptionVariation:SendToRabbitMQ ; Body: " + JsonConvert.SerializeObject(param));
                }

                return new JsonResult(userVariation.Variation);
            }
            catch(Exception exp)
            {
                _logger.LogError(exp, "Post /Variation/GetMultiOptionVariation ; Body: " + JsonConvert.SerializeObject(param));
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return new JsonResult(exp.Message);
            }
        }
        
        private void SendToRabbitMQ(GetUserVariationResultParam param, FeatureFlagIdByEnvironmentKeyViewModel ffIdVM, UserVariation userVariation)
        {
            var variation = userVariation.Variation;
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
                VariationLocalId = variation.LocalId.ToString(),
                VariationValue = variation.VariationValue,
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
                                  LabelValue = variation.LocalId.ToString()
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "VariationValue",
                                  LabelValue = variation.VariationValue
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

            _messagingService.SendAPIServiceToMQServiceWithoutResponse(new APIServiceToMQServiceModel
            {
                SendToExperiment = userVariation.SendToExperiment,
                FFMessage = ffEvent,
                Message = new FeatureFlagsCo.MQ.MessageModel
                {
                    SendDateTime = DateTime.UtcNow,
                    Labels = labels,
                    Message = JsonConvert.SerializeObject(param ?? new GetUserVariationResultParam()),
                    FeatureFlagId = ffIdVM.FeatureFlagId,
                    IndexTarget = "ffvariationrequestindex"
                }
            });
        }
    }
}
