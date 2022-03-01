using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.Utils.ExtensionMethods;
using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlagsCo.MQ;
using System;
using Newtonsoft.Json;
using AutoMapper;

namespace FeatureFlags.APIs.Repositories
{
    public interface IFeatureFlagsService
    {
        Task<EnvironmentUserQueryResultViewModel> QueryEnvironmentFeatureFlagUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize, string currentUserId);
        void SendFeatureFlagUsageToMQ(InsightParam param, FeatureFlagIdByEnvironmentKeyViewModel ffIdVM, InsightUserVariationParam insightUserVariation);
    }

    public class FeatureFlagsService : IFeatureFlagsService
    {
        private readonly INoSqlService _cosmosDbService;
        private readonly MessagingService _messagingService;
        private readonly IMapper _mapper;

        public FeatureFlagsService(
            INoSqlService cosmosDbService,
            MessagingService messagingService,
            IMapper mapper)
        {
            _cosmosDbService = cosmosDbService;
            _messagingService = messagingService;
            _mapper = mapper;
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

        public void SendFeatureFlagUsageToMQ(InsightParam param, FeatureFlagIdByEnvironmentKeyViewModel ffIdVM, InsightUserVariationParam insightUserVariation)
        {
            var variation = insightUserVariation.Variation;
            var ffEvent = new FeatureFlagMessageModel()
            {
                RequestPath = "/Variation/GetMultiOptionVariation",
                FeatureFlagId = ffIdVM.FeatureFlagId,
                EnvId = ffIdVM.EnvId,
                AccountId = ffIdVM.AccountId,
                ProjectId = ffIdVM.ProjectId,
                FeatureFlagKeyName = insightUserVariation.FeatureFlagKeyName,
                UserKeyId = param.User.KeyId,
                FFUserName = param.User.UserName,
                VariationLocalId = variation.LocalId.ToString(),
                VariationValue = variation.VariationValue,
                TimeStamp = insightUserVariation.Timestamp.UnixTimestampInMillisecondsToDateTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffff")
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
                                  LabelValue = insightUserVariation.FeatureFlagKeyName
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "UserKeyId",
                                  LabelValue = param.User.KeyId
                              },
                              new FeatureFlagsCo.MQ.MessageLabel
                              {
                                  LabelName = "FFUserName",
                                  LabelValue = param.User.UserName
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
            if (param.CustomizedProperties != null && param.CustomizedProperties.Count > 0)
            {
                foreach (var item in param.CustomizedProperties)
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
                SendToExperiment = insightUserVariation.SendToExperiment,
                FFMessage = ffEvent,
                Message = new FeatureFlagsCo.MQ.MessageModel
                {
                    SendDateTime = DateTime.UtcNow,
                    Labels = labels,
                    Message = JsonConvert.SerializeObject(param != null ? _mapper.Map<InsightParam, FeatureFlagUsage>(param) : new FeatureFlagUsage()),
                    FeatureFlagId = ffIdVM.FeatureFlagId,
                    IndexTarget = "ffvariationrequestindex"
                }
            });
        }
    }
}
