using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.Utils.ExtensionMethods;
using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlagsCo.MQ;
using System;
using Newtonsoft.Json;

namespace FeatureFlags.APIs.Repositories
{
    public interface IFeatureFlagsService
    {
        Task<EnvironmentUserQueryResultViewModel> QueryEnvironmentFeatureFlagUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize, string currentUserId);
        void SendFeatureFlagUsageToMQ(FeatureFlagUsageParam param, FeatureFlagIdByEnvironmentKeyViewModel ffIdVM, UserVariation userVariation, long timestamp);
    }

    public class FeatureFlagsService : IFeatureFlagsService
    {
        private readonly INoSqlService _cosmosDbService;
        private readonly MessagingService _messagingService;

        public FeatureFlagsService(
            INoSqlService cosmosDbService,
            MessagingService messagingService)
        {
            _cosmosDbService = cosmosDbService;
            _messagingService = messagingService;
        }

        public async Task<EnvironmentUserQueryResultViewModel> QueryEnvironmentFeatureFlagUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize, string currentUserId)
        {
            var users = await _cosmosDbService.QueryEnvironmentUsersAsync(searchText, environmentId, pageIndex, pageSize);
            int pageTotalNumber = await _cosmosDbService.QueryEnvironmentUsersCountAsync(searchText, environmentId, pageIndex, pageSize);
            return new EnvironmentUserQueryResultViewModel
            {
                Count = pageTotalNumber,
                Users = users
            };
        }

        public void SendFeatureFlagUsageToMQ(FeatureFlagUsageParam param, FeatureFlagIdByEnvironmentKeyViewModel ffIdVM, UserVariation userVariation, long timestamp)
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
                UserKeyId = param.UserKeyId,
                FFUserName = param.UserName,
                VariationLocalId = variation.LocalId.ToString(),
                VariationValue = variation.VariationValue,
                TimeStamp = timestamp.UnixTimestampInMillisecondsToDateTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffff")
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
                                  LabelValue = param.UserKeyId
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "FFUserName",
                                  LabelValue = param.UserName
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
            if (param.UserCustomizedProperties != null && param.UserCustomizedProperties.Count > 0)
            {
                foreach (var item in param.UserCustomizedProperties)
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
                    Message = JsonConvert.SerializeObject(param ?? new FeatureFlagUsageParam()),
                    FeatureFlagId = ffIdVM.FeatureFlagId,
                    IndexTarget = "ffvariationrequestindex"
                }
            });
        }
    }
}
