using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.Utils.ExtensionMethods;
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
        private readonly MongoDbFeatureFlagService _mongoDbFeatureFlagService;
        private readonly IFeatureFlagsService _featureFlagService;

        public VariationController(
            ILogger<VariationController> logger, 
            IVariationService variationService,
            MongoDbFeatureFlagService mongoDbFeatureFlagService,
            IFeatureFlagsService featureFlagService,
            MessagingService messagingService)
        {
            _logger = logger;
            _variationService = variationService;
            _featureFlagService = featureFlagService;
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
                    var featureFlagUsage = new FeatureFlagUsageParam
                    {
                        FeatureFlagKeyName = param.FeatureFlagKeyName,
                        UserName = param.FFUserName,
                        Email = param.FFUserEmail,
                        Country = param.FFUserCountry,
                        UserKeyId = param.FFUserKeyId,
                        UserCustomizedProperties = param.FFUserCustomizedProperties
                    };

                    _featureFlagService.SendFeatureFlagUsageToMQ(featureFlagUsage, ffIdVM, new CachedUserVariation(variation, true), DateTime.UtcNow.UnixTimestampInMilliseconds());
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
                var userVariation = await _variationService.GetUserVariationAsync(
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
                    var featureFlagUsage = new FeatureFlagUsageParam
                    {
                        FeatureFlagKeyName = param.FeatureFlagKeyName,
                        UserName = param.FFUserName,
                        Email = param.FFUserEmail,
                        Country = param.FFUserCountry,
                        UserKeyId = param.FFUserKeyId,
                        UserCustomizedProperties = param.FFUserCustomizedProperties
                    };

                    _featureFlagService.SendFeatureFlagUsageToMQ(featureFlagUsage, ffIdVm, userVariation, DateTime.UtcNow.UnixTimestampInMilliseconds());
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
    }
}
