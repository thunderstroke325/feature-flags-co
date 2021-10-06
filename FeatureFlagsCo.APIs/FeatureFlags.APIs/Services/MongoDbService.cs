using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.DataSync;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.ViewModels.FeatureFlagTrigger;
using Microsoft.AspNetCore.Mvc;
using FeatureFlags.APIs.ViewModels.FeatureFlagCommit;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbService : INoSqlService
    {
        private readonly MongoDbFeatureFlagService _mongoFeatureFlagsService;
        private readonly MongoDbEnvironmentUserService _mongoEnvironmentUsersService;
        private readonly MongoDbEnvironmentUserPropertyService _mongoEnvironmentUserPropertiesService;
        private readonly MongoDbFeatureTriggerService _mongoDbFeatureTriggerService;
        private readonly MongoDbExperimentService _mongoDbExperimentService;
        //private readonly MongoDbFeatureFlagCommitService _mongoDbFeatureFlagCommitService;

        private readonly IJwtUtilsService _jwtUtilsService;

        public MongoDbService(
            MongoDbFeatureFlagService mongoFeatureFlagsService,
            MongoDbEnvironmentUserService mongoEnvironmentUsersService,
            MongoDbEnvironmentUserPropertyService mongoEnvironmentUserPropertiesService,
            MongoDbFeatureTriggerService mongoDbFeatureTriggerService,
            MongoDbExperimentService mongoDbExperimentService,
            //MongoDbFeatureFlagCommitService mongoDbFeatureFlagCommitService,
            IJwtUtilsService jwtUtilsService)
        {
            _mongoFeatureFlagsService = mongoFeatureFlagsService;
            _mongoEnvironmentUsersService = mongoEnvironmentUsersService;
            _mongoEnvironmentUserPropertiesService = mongoEnvironmentUserPropertiesService;
            _mongoDbFeatureTriggerService = mongoDbFeatureTriggerService;
            _mongoDbExperimentService = mongoDbExperimentService;
            //_mongoDbFeatureFlagCommitService = mongoDbFeatureFlagCommitService;
            _jwtUtilsService = jwtUtilsService;
        }

        #region experiments

        public async Task<List<Experiment>> GetExperimentsByIdsAsync(List<string> featureFlagIds) 
        {
            return await _mongoDbExperimentService.GetByIdsAsync(featureFlagIds);
        }

        public async Task<Experiment> GetExperimentByIdAsync(string exptId)
        {
            return await _mongoDbExperimentService.GetAsync(exptId);
        }

        public async Task<Experiment> GetExperimentByFeatureFlagAndMetricAsync(string featureFlagId, string metricId)
        {
            return (await _mongoDbExperimentService.GetByFeatureFlagAndMetricAsync(featureFlagId, metricId)).FirstOrDefault();
        }

        public async Task<List<Experiment>> GetExperimentByFeatureFlagAsync(string featureFlagId)
        {
            return await _mongoDbExperimentService.GetByFeatureFlagAsync(featureFlagId);
        }

        // This method is used for updating Experiment
        public async Task<Experiment> UpsertExperimentAsync(Experiment item)
        {
            item.UpdatedAt = DateTime.UtcNow;
            return await _mongoDbExperimentService.UpsertItemAsync(item);
        }

        public async Task<Experiment> CreateExperimentAsync(Experiment item)
        {
            var now = DateTime.UtcNow;
            item.CreatedAt = now;
            item.UpdatedAt = now;
            return await _mongoDbExperimentService.CreateAsync(item);
        }

        public async Task<Experiment> ArchiveExperimentAsync(string exptId, DateTime stopTime)
        {
            var experiment = await _mongoDbExperimentService.GetAsync(exptId);
            if (experiment != null)
            {
                experiment.IsArvhived = true;
                experiment.UpdatedAt = DateTime.UtcNow;
                experiment.Iterations.ForEach(i => i.EndTime = stopTime);
                await _mongoDbExperimentService.UpsertItemAsync(experiment);
                return experiment;
            }

            return null;
        }

        #endregion


        #region approval
        //public async Task<List<FeatureFlagCommit>> GetApprovalRequestsAsync(string featureFlagId)
        //{
        //    // Get all feature flag commit based on review status = Pending | Approved
        //    return await _mongoDbFeatureFlagCommitService.GetApprovalRequestsAsync(featureFlagId, 0, 100);
        //}
        //public async Task<FeatureFlagCommit> GetApprovalRequestAsync(string featureFlagCommitId)
        //{
        //    return await _mongoDbFeatureFlagCommitService.GetAsync(featureFlagCommitId);
        //}
        //public async Task<bool> CreateApproveRequestAsync(CreateApproveRequestParam param, string userId)
        //{
        //    var oldFF = await _mongoFeatureFlagsService.GetAsync(param.FeatureFlagParam.Id);

        //    var ffc = new FeatureFlagCommit();
        //    ffc.CreatedFromFeatureFlag(param.FeatureFlagParam);
        //    ffc.Version = Guid.NewGuid().ToString();
        //    ffc.RequestUserId = userId;
        //    ffc.CreatedAt = DateTime.UtcNow;
        //    ffc.FeatureFlagId = param.FeatureFlagParam.Id;
        //    ffc.Id = Guid.NewGuid().ToString();
        //    ffc.ActivityLogs = new System.Collections.Generic.List<ActivityLog>()
        //    {
        //        new ActivityLog
        //        {
        //            ActivityType = ActivityTypeEnum.Create,
        //            Comment = param.Comment,
        //            CreatedAt = DateTime.UtcNow,
        //            UserId = userId
        //        }
        //    };
        //    await _mongoDbFeatureFlagCommitService.CreateAsync(ffc);
        //    return true;
        //}
        //public async Task<bool> ApproveApprovalRequestAsync(ApproveRequestParam arParam, string userId)
        //{
        //    var ffc = await GetApprovalRequestAsync(arParam.FeatureFlagCommitId);
        //    ffc.ApprovalRequest.ApprovedByUserIds.Add(userId);
        //    ffc.ApprovalRequest.ReviewStatus = ReviewStatusEnum.Approved;
        //    ffc.ActivityLogs.Add(new ActivityLog
        //    {
        //        Comment = arParam.Comment,
        //        ActivityType = ActivityTypeEnum.Approve,
        //        CreatedAt = DateTime.UtcNow,
        //        UserId = userId
        //    });
        //    await _mongoDbFeatureFlagCommitService.UpdateAsync(ffc.Id, ffc);
        //    return true;
        //}
        //public async Task<bool> ChangeApprovalRequestAsync(ChangeRequestParam arParam, string userId)
        //{
        //    var ffc = await GetApprovalRequestAsync(arParam.FeatureFlagCommitId);
        //    if (ffc.ApprovalRequest.ReviewStatus != ReviewStatusEnum.Approved)
        //        return false;
        //    if (ffc.ApprovalRequest.ApprovedByUserIds.Contains(userId))
        //        ffc.ApprovalRequest.ApprovedByUserIds.Remove(userId);
        //    if (ffc.ApprovalRequest.ApprovedByUserIds == null ||
        //        ffc.ApprovalRequest.ApprovedByUserIds.Count == 0)
        //        ffc.ApprovalRequest.ReviewStatus = ReviewStatusEnum.Pending;
        //    ffc.ActivityLogs.Add(new ActivityLog
        //    {
        //        Comment = arParam.Comment,
        //        ActivityType = ActivityTypeEnum.ChangeRequest,
        //        CreatedAt = DateTime.UtcNow,
        //        UserId = userId
        //    });
        //    await _mongoDbFeatureFlagCommitService.UpdateAsync(ffc.Id, ffc);
        //    return true;
        //}
        //public async Task<bool> DeclineApprovalRequestAsync(DeclineRequestParam arParam, string userId)
        //{
        //    var ffc = await GetApprovalRequestAsync(arParam.FeatureFlagCommitId);
        //    ffc.ApprovalRequest.ApprovedByUserIds.Add(userId);
        //    ffc.ApprovalRequest.ReviewStatus = ReviewStatusEnum.Declined;
        //    ffc.ActivityLogs.Add(new ActivityLog
        //    {
        //        Comment = arParam.Comment,
        //        ActivityType = ActivityTypeEnum.Decline,
        //        CreatedAt = DateTime.UtcNow,
        //        UserId = userId
        //    });
        //    await _mongoDbFeatureFlagCommitService.UpdateAsync(ffc.Id, ffc);
        //    return true;
        //}
        //public async Task<FeatureFlag> ApplyApprovalRequestAsync(ApplyRequestParam arParam, string userId)
        //{
        //    var ffc = await GetApprovalRequestAsync(arParam.FeatureFlagCommitId);
        //    if (ffc.ApprovalRequest.ApprovedByUserIds.Contains(userId))
        //    {
        //        ffc.ApprovalRequest.ReviewStatus = ReviewStatusEnum.Applied;
        //        ffc.EffeciveDate = DateTime.UtcNow;
        //        ffc.ActivityLogs.Add(new ActivityLog
        //        {
        //            Comment = arParam.Comment,
        //            ActivityType = ActivityTypeEnum.Apply,
        //            CreatedAt = DateTime.UtcNow,
        //            UserId = userId
        //        });
        //        await _mongoDbFeatureFlagCommitService.UpdateAsync(ffc.Id, ffc);

        //        var ff = await _mongoFeatureFlagsService.GetAsync(ffc.FeatureFlagId);
        //        ff.FF = ffc.FF;
        //        ff.FFP = ffc.FFP;
        //        ff.FFTUWMTR = ffc.FFTUWMTR;
        //        ff.EffeciveDate = ffc.EffeciveDate;
        //        ff.TargetIndividuals = ffc.TargetIndividuals;
        //        ff.VariationOptions = ffc.VariationOptions;
        //        ff.Version = ffc.Version;
        //        return ff;
        //    }
        //    return null;
        //}
        //private async Task BackupFeatureflagDirectlyUpdateAsync(FeatureFlag ff)
        //{
        //    var ffc = new FeatureFlagCommit();
        //    ffc.CreatedFromFeatureFlag(ff);
        //    await _mongoDbFeatureFlagCommitService.CreateAsync(ffc);
        //}
        #endregion


        public async Task SaveEnvironmentDataAsync(int accountId, int projectId, int envId,
            EnvironmentDataViewModel data)
        {
            data.FeatureFlags.ForEach(async ff =>
            {
                var keyName = FeatureFlagKeyExtension.CreateNewFeatureFlagKeyName(envId, ff.FF.Name);
                ff.Id = FeatureFlagKeyExtension.GetFeatureFlagId(keyName, envId.ToString(), accountId.ToString(),
                    projectId.ToString());
                ff.EnvironmentId = envId;
                ff.FF.EnvironmentId = envId;

                ff.FF.Id = ff.Id;
                ff._Id = null;
                await this._mongoFeatureFlagsService.UpsertItemAsync(ff);
            });

            data.EnvironmentUsers.ForEach(async u =>
            {
                u.EnvironmentId = envId;
                u._Id = null;
                await this._mongoEnvironmentUsersService.UpsertItemAsync(u);
            });

            if (data.EnvironmentUserProperties != null)
            {
                data.EnvironmentUserProperties.EnvironmentId = envId;
                data.EnvironmentUserProperties._Id = null;
                await this._mongoEnvironmentUserPropertiesService.UpsertItemAsync(data.EnvironmentUserProperties);
            }
        }


        public async Task<List<T>> GetEnvironmentDataAsync<T>(int envId)
        {
            var result = new List<T>();

            if (typeof(T) == typeof(FeatureFlag))
            {
                result = (await _mongoFeatureFlagsService.GetByEnvironmentAsync(envId)) as List<T>;
            }
            else if (typeof(T) == typeof(EnvironmentUser))
            {
                result = (await _mongoEnvironmentUsersService.GetByEnvironmentAsync(envId)) as List<T>;
            }
            else if (typeof(T) == typeof(EnvironmentUserProperty))
            {
                result = (await _mongoEnvironmentUserPropertiesService.GetByEnvironmentAsync(envId)) as List<T>;
            }

            return result;
        }

        public async Task<EnvironmentUser> AddEnvironmentUserAsync(EnvironmentUser item)
        {
            var newItem = await _mongoEnvironmentUsersService.CreateAsync(item);
            return newItem;
        }

        public async Task<FeatureFlag> GetFeatureFlagAsync(string id)
        {
            return await _mongoFeatureFlagsService.GetAsync(id);
        }

        public async Task<EnvironmentUser> GetEnvironmentUserAsync(string id)
        {
            try
            {
                return await _mongoEnvironmentUsersService.GetAsync(id);
            }
            catch (MongoException)
            {
                return null;
            }
        }

        public async Task<FeatureFlag> CreateDemoFeatureFlagAsync(CreateFeatureFlagViewModel param, string currentUserId,
           int projectId, int accountId)
        {
            var keyName = FeatureFlagKeyExtension.CreateNewFeatureFlagKeyName(param.EnvironmentId, param.Name);
            var featureFlagId = FeatureFlagKeyExtension.GetFeatureFlagId(keyName, param.EnvironmentId.ToString(),
                accountId.ToString(), projectId.ToString());
            var newFeatureFlag = new FeatureFlag()
            {
                Id = featureFlagId,
                EnvironmentId = param.EnvironmentId,
                FF = new FeatureFlagBasicInfo
                {
                    Id = featureFlagId,
                    LastUpdatedTime = DateTime.UtcNow,
                    KeyName = keyName,
                    EnvironmentId = param.EnvironmentId,
                    CreatorUserId = currentUserId,
                    Name = param.Name,
                    Status = param.Status,
                    VariationOptionWhenDisabled = new VariationOption()
                    {
                        DisplayOrder = 1,
                        LocalId = 1,
                        VariationValue = "产品经理话术版1"
                    },
                    DefaultRulePercentageRollouts = new List<VariationOptionPercentageRollout>()
                    {
                        new VariationOptionPercentageRollout
                        {
                            RolloutPercentage = new double[2] {0, 0.33},
                            ValueOption = new VariationOption()
                            {
                                DisplayOrder = 1,
                                LocalId = 1,
                                VariationValue = "产品经理话术版1"
                            }
                        },
                        new VariationOptionPercentageRollout
                        {
                            RolloutPercentage = new double[2] {0.33, 0.67},
                            ValueOption = new VariationOption()
                            {
                                DisplayOrder = 2,
                                LocalId = 2,
                                VariationValue = "程序员话术版1"
                            }
                        },
                        new VariationOptionPercentageRollout
                        {
                            RolloutPercentage = new double[2] {0.67, 1},
                            ValueOption = new VariationOption()
                            {
                                DisplayOrder = 3,
                                LocalId = 3,
                                VariationValue = "产品经理话术版2"
                            }
                        }
                    }
                },
                IsArchived = false,
                FFP = new List<FeatureFlagPrerequisite>(),
                FFTUWMTR = new List<FeatureFlagTargetUsersWhoMatchTheseRuleParam>() 
                {
                    new FeatureFlagTargetUsersWhoMatchTheseRuleParam 
                    {
                        RuleName = "CSDN 规则",
                        RuleJsonContent = new List<FeatureFlagRuleJsonContent> 
                        {
                            new FeatureFlagRuleJsonContent 
                            {
                                Operation = "EndsWith",
                                Property = "外放地址",
                                Value = "?from=csdn"
                            }
                        },
                        ValueOptionsVariationRuleValues = new List<VariationOptionPercentageRollout> 
                        {
                            new VariationOptionPercentageRollout 
                            {
                                RolloutPercentage = new double[2] {0, 1},
                                ValueOption = new VariationOption()
                                {
                                    DisplayOrder = 2,
                                    LocalId = 2,
                                    VariationValue = "程序员话术版1"
                                }
                            }
                        }
                    },
                    new FeatureFlagTargetUsersWhoMatchTheseRuleParam
                    {
                        RuleName = "RRCPJL 规则",
                        RuleJsonContent = new List<FeatureFlagRuleJsonContent>
                        {
                            new FeatureFlagRuleJsonContent
                            {
                                Operation = "EndsWith",
                                Property = "外放地址",
                                Value = "?from=rrcpjl"
                            }
                        },
                        ValueOptionsVariationRuleValues = new List<VariationOptionPercentageRollout>
                        {
                            new VariationOptionPercentageRollout
                            {
                                RolloutPercentage = new double[2] {0, 0.5},
                                ValueOption = new VariationOption()
                                {
                                    DisplayOrder = 1,
                                    LocalId = 1,
                                    VariationValue = "产品经理话术版1"
                                }
                            },
                            new VariationOptionPercentageRollout
                            {
                                RolloutPercentage = new double[2] {0.5, 0.5},
                                ValueOption = new VariationOption()
                                {
                                    DisplayOrder = 2,
                                    LocalId = 2,
                                    VariationValue = "程序员话术版1"
                                }
                            },
                            new VariationOptionPercentageRollout
                            {
                                RolloutPercentage = new double[2] {0, 0.5},
                                ValueOption = new VariationOption()
                                {
                                    DisplayOrder = 3,
                                    LocalId = 3,
                                    VariationValue = "产品经理话术版2"
                                }
                            }
                        }
                    }
                },
                VariationOptions = new List<VariationOption>()
                {
                    new VariationOption()
                    {
                        DisplayOrder = 1,
                        LocalId = 1,
                        VariationValue = "产品经理话术版1"
                    },
                    new VariationOption()
                    {
                        DisplayOrder = 2,
                        LocalId = 2,
                        VariationValue = "程序员话术版1"
                    },
                    new VariationOption()
                    {
                        DisplayOrder = 3,
                        LocalId = 3,
                        VariationValue = "产品经理话术版2"
                    },
                },
                TargetIndividuals = new List<TargetIndividualForVariationOption>()
            };
            return await _mongoFeatureFlagsService.CreateAsync(newFeatureFlag);
        }

        public async Task<FeatureFlag> CreateFeatureFlagAsync(CreateFeatureFlagViewModel param, string currentUserId,
            int projectId, int accountId)
        {
            var keyName = FeatureFlagKeyExtension.CreateNewFeatureFlagKeyName(param.EnvironmentId, param.Name);
            var featureFlagId = FeatureFlagKeyExtension.GetFeatureFlagId(keyName, param.EnvironmentId.ToString(),
                accountId.ToString(), projectId.ToString());
            var newFeatureFlag = new FeatureFlag()
            {
                Id = featureFlagId,
                EnvironmentId = param.EnvironmentId,
                FF = new FeatureFlagBasicInfo
                {
                    Id = featureFlagId,
                    LastUpdatedTime = DateTime.UtcNow,
                    KeyName = keyName,
                    EnvironmentId = param.EnvironmentId,
                    CreatorUserId = currentUserId,
                    Name = param.Name,
                    Status = param.Status,
                    VariationOptionWhenDisabled = new VariationOption()
                    {
                        DisplayOrder = 1,
                        LocalId = 1,
                        VariationValue = "true"
                    },
                    DefaultRulePercentageRollouts = new List<VariationOptionPercentageRollout>()
                    {
                        new VariationOptionPercentageRollout
                        {
                            RolloutPercentage = new double[2] {0, 1},
                            ValueOption = new VariationOption()
                            {
                                DisplayOrder = 1,
                                LocalId = 1,
                                VariationValue = "true"
                            }
                        }
                    }
                },
                IsArchived = false,
                FFP = new List<FeatureFlagPrerequisite>(),
                FFTUWMTR = new List<FeatureFlagTargetUsersWhoMatchTheseRuleParam>(),
                VariationOptions = new List<VariationOption>()
                {
                    new VariationOption()
                    {
                        DisplayOrder = 1,
                        LocalId = 1,
                        VariationValue = "true"
                    },
                    new VariationOption()
                    {
                        DisplayOrder = 2,
                        LocalId = 2,
                        VariationValue = "false"
                    },
                },
                TargetIndividuals = new List<TargetIndividualForVariationOption>()
            };
            return await _mongoFeatureFlagsService.CreateAsync(newFeatureFlag);
        }


        public async Task<ReturnJsonModel<FeatureFlag>> UpdateMultiValueOptionSupportedFeatureFlagAsync(
            FeatureFlag param)
        {
            try
            {
                if (param.FF.DefaultRulePercentageRollouts == null || param.FF.DefaultRulePercentageRollouts.Count == 0)
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception(
                            "In Multi Option supported mode, DefaultRulePercentageRollouts shouldn't be empty")
                    };
                if (param.FF.VariationOptionWhenDisabled == null)
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception(
                            "In Multi Option supported mode, MultiOptionValueWhenDisabled shouldn't be empty")
                    };
                if (param.FFP != null && param.FFP.Count > 0 &&
                    param.FFP.Any(p => p.ValueOptionsVariationValue == null))
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception(
                            "In Multi Option supported mode, all ValueOptionsVariationValue FFPs shouldn't be empty")
                    };
                if (param.FFTUWMTR != null && param.FFTUWMTR.Count > 0 && param.FFTUWMTR.Any(p =>
                    p.ValueOptionsVariationRuleValues == null || p.ValueOptionsVariationRuleValues.Count == 0))
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception(
                            "In Multi Option supported mode, ValueOptionsVariationRuleValues in all FFTUWMTRs shouldn't be empty")
                    };
                if (param.VariationOptions == null || param.VariationOptions.Count == 0)
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, VariationOptions shouldn't be empty")
                    };
                if (param.TargetIndividuals != null && param.TargetIndividuals.Count >= 0 &&
                    param.TargetIndividuals.Any(p => p.ValueOption == null))
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception(
                            "In Multi Option supported mode, ValueOption in TargetIndividual shouldn't be empty")
                    };

                param.EnvironmentId = param.FF.EnvironmentId;
                param.Id = param.FF.Id;
                param.FF.LastUpdatedTime = DateTime.UtcNow;

                if (param.FFTUWMTR != null && param.FFTUWMTR.Count > 0)
                {
                    foreach (var item in param.FFTUWMTR)
                    {
                        if (string.IsNullOrWhiteSpace(item.RuleId))
                        {
                            item.RuleId = Guid.NewGuid().ToString();
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(param._Id))
                {
                    var ffInDb = await _mongoFeatureFlagsService.GetAsync(param.Id);
                    param._Id = ffInDb._Id;
                }

                await _mongoFeatureFlagsService.UpdateAsync(param.Id, param);

                return new ReturnJsonModel<FeatureFlag>
                {
                    StatusCode = 200,
                    Data = param
                };
            }
            catch (Exception exp)
            {
                return new ReturnJsonModel<FeatureFlag>
                {
                    StatusCode = 500,
                    Error = exp
                };
            }
        }

        public async Task<FeatureFlag> UpdateFeatureFlagAsync(FeatureFlag param)
        {
            var originFF = await this.GetFlagAsync(param.Id);
            param.EnvironmentId = param.FF.EnvironmentId;
            param.Id = param.FF.Id;
            param.FF.LastUpdatedTime = DateTime.UtcNow;
            if (param.FFTUWMTR != null && param.FFTUWMTR.Count > 0)
            {
                foreach (var item in param.FFTUWMTR)
                {
                    if (string.IsNullOrWhiteSpace(item.RuleId))
                    {
                        item.RuleId = Guid.NewGuid().ToString();
                    }
                }
            }

            param._Id = originFF._Id;
            await _mongoFeatureFlagsService.UpdateAsync(param.Id, param);

            //await BackupFeatureflagDirectlyUpdateAsync(param);

            return param;
        }

        public async Task<FeatureFlag> ArchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param)
        {
            var originFF = await this.GetFlagAsync(param.FeatureFlagId);
            originFF.FF.LastUpdatedTime = DateTime.UtcNow;
            originFF.IsArchived = true;
            originFF.FF.Status = FeatureFlagStatutEnum.Disabled.ToString();

            await _mongoFeatureFlagsService.UpdateAsync(originFF.Id, originFF);
            return originFF;
        }

        public async Task<FeatureFlag> UnarchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param)
        {
            var originFF = await this.GetFlagAsync(param.FeatureFlagId);
            originFF.FF.LastUpdatedTime = DateTime.UtcNow;
            originFF.IsArchived = false;

            await _mongoFeatureFlagsService.UpdateAsync(originFF.Id, originFF);
            return originFF;
        }

        public async Task<EnvironmentUserProperty> UpdateEnvironmentUserPropertiesAsync(int environmentId,
            List<string> propertyName)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(environmentId);
            EnvironmentUserProperty environmentUserProperty = null;
            try
            {
                environmentUserProperty = await _mongoEnvironmentUserPropertiesService.GetAsync(id);
                if (propertyName != null && propertyName.Count > 0)
                {
                    foreach (var name in propertyName)
                    {
                        environmentUserProperty.Properties.Add(name);
                    }
                }

                environmentUserProperty.Properties = environmentUserProperty.Properties.Distinct().ToList();
                await _mongoEnvironmentUserPropertiesService.UpdateAsync(environmentUserProperty.Id,
                    environmentUserProperty);
            }
            catch (MongoException)
            {
                environmentUserProperty = (await _mongoEnvironmentUserPropertiesService.CreateAsync(
                    new EnvironmentUserProperty
                    {
                        Id = id,
                        Properties = propertyName ?? new List<string>(),
                        EnvironmentId = environmentId
                    }));
            }

            return environmentUserProperty;
        }

        public async Task<EnvironmentUserProperty> GetEnvironmentUserPropertiesAsync(int environmentId)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(environmentId);
            try
            {
                EnvironmentUserProperty returnModel = await _mongoEnvironmentUserPropertiesService.GetAsync(id);
                if (returnModel == null)
                    returnModel = new EnvironmentUserProperty()
                    {
                        EnvironmentId = environmentId,
                        Properties = new List<string>()
                    };
                returnModel.Properties.Add("KeyId");
                returnModel.Properties.Add("Name");
                returnModel.Properties.Add("Email");
                return returnModel;
            }
            catch (MongoException)
            {
                return new EnvironmentUserProperty()
                {
                    Properties = new List<string>()
                };
            }
        }


        public async Task<FeatureFlag> GetFlagAsync(string id)
        {
            return await _mongoFeatureFlagsService.GetAsync(id);
        }

        public async Task<List<FeatureFlagBasicInfo>> GetEnvironmentFeatureFlagBasicInfoItemsAsync(int environmentId,
            int pageIndex = 0, int pageSize = 100)
        {
            var returnResult = new List<FeatureFlagBasicInfo>();
            var ffs = await _mongoFeatureFlagsService.GetFeatureFlagsAsync(environmentId, false, pageIndex, pageSize);
            foreach (var ff in ffs)
            {
                var ffb = ff.FF;
                if (ffb != null)
                {
                    ffb.Status = string.IsNullOrWhiteSpace(ffb.Status)
                        ? FeatureFlagStatutEnum.Enabled.ToString()
                        : ffb.Status;
                    returnResult.Add(ffb);
                }
            }

            return returnResult;
        }

        public async Task<List<FeatureFlagBasicInfo>> GetEnvironmentArchivedFeatureFlagBasicInfoItemsAsync(
            int environmentId, int pageIndex = 0, int pageSize = 100)
        {
            var returnResult = new List<FeatureFlagBasicInfo>();
            var ffs = await _mongoFeatureFlagsService.GetFeatureFlagsAsync(environmentId, true, pageIndex, pageSize);
            foreach (var ff in ffs)
            {
                var ffb = ff.FF;
                if (ffb != null)
                {
                    ffb.Status = string.IsNullOrWhiteSpace(ffb.Status)
                        ? FeatureFlagStatutEnum.Enabled.ToString()
                        : ffb.Status;
                    returnResult.Add(ffb);
                }
            }

            return returnResult;
        }


        public async Task<int> QueryEnvironmentUsersCountAsync(string searchText, int environmentId, int pageIndex,
            int pageSize)
        {
            return await _mongoEnvironmentUsersService.CountAsync(searchText, environmentId);
        }


        public async Task<List<EnvironmentUser>> QueryEnvironmentUsersAsync(string searchText, int environmentId,
            int pageIndex, int pageSize)
        {
            return await _mongoEnvironmentUsersService.SearchAsync(searchText, environmentId, pageIndex, pageSize);
        }

        public async Task<EnvironmentUserProperty> GetEnvironmentUserPropertiesForCRUDAsync(int environmentId)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(environmentId);
            try
            {
                EnvironmentUserProperty returnModel = await _mongoEnvironmentUserPropertiesService.GetAsync(id);
                if (returnModel == null)
                    returnModel = new EnvironmentUserProperty()
                    {
                        EnvironmentId = environmentId,
                        Properties = new List<string>()
                    };

                return returnModel;
            }
            catch (MongoException)
            {
                return new EnvironmentUserProperty()
                {
                    EnvironmentId = environmentId,
                    Properties = new List<string>()
                };
            }
        }

        public async Task CreateOrUpdateEnvironmentUserPropertiesForCRUDAsync(EnvironmentUserProperty param)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(param.EnvironmentId);
            var eup = await this.GetEnvironmentUserPropertiesAsync(param.EnvironmentId);
            if (string.IsNullOrWhiteSpace(eup._Id))
            {
                await _mongoEnvironmentUserPropertiesService.CreateAsync(new EnvironmentUserProperty
                {
                    Id = id,
                    EnvironmentId = param.EnvironmentId,
                    Properties = param.Properties,
                    ObjectType = param.ObjectType
                });
            }
            else
            {
                await _mongoEnvironmentUserPropertiesService.UpdateAsync(id, new EnvironmentUserProperty
                {
                    _Id = eup._Id,
                    Id = id,
                    EnvironmentId = param.EnvironmentId,
                    Properties = param.Properties,
                    ObjectType = param.ObjectType
                });
            }
        }

        public async Task UpsertEnvironmentUserAsync(EnvironmentUser param)
        {
            await _mongoEnvironmentUsersService.UpsertAsync(param);
        }

        public async Task<List<FeatureFlagTrigger>> GetFlagTriggersByFfIdAsync(string id)
        {
            return await _mongoDbFeatureTriggerService.GetByFeatureFlagIdAsync(id);
        }

        public async Task TriggerFeatureFlagByFlagTriggerAsync(string token)
        {
            
            var id = _jwtUtilsService.ValidateToken(token);
            if(id == null) throw new InvalidOperationException("Invalid token");
            var trigger = await _mongoDbFeatureTriggerService.GetByTokenAsync(id, token);
            if (trigger != null && trigger.Status == (int)FeatureFlagTriggerStatusEnum.Enabled)
            {
                var featureFlag = await _mongoFeatureFlagsService.GetAsync(trigger.FeatureFlagId);
                if (featureFlag != null)
                {
                    switch (trigger.Action)
                    {
                        case (int)FeatureFlagTriggerActionEnum.On:
                            featureFlag.FF.Status = FeatureFlagStatutEnum.Enabled.ToString();
                            break;
                        case (int)FeatureFlagTriggerActionEnum.Off:
                            featureFlag.FF.Status = FeatureFlagStatutEnum.Disabled.ToString();
                            break;
                        default:
                            throw new NotSupportedException("action doesn't be supported");
                    }

                    featureFlag.FF.LastUpdatedTime = trigger.LastTriggeredAt = DateTime.UtcNow;
                    trigger.Times += 1;
                    await _mongoFeatureFlagsService.UpsertItemAsync(featureFlag);
                    await _mongoDbFeatureTriggerService.UpsertAsync(trigger);
                }
                else
                {
                    throw new NotSupportedException("trigger doesn't work anymore");
                }
            }
            else
            {
                throw new NotSupportedException("trigger doesn't work anymore");
            }
        }

        public async Task<FeatureFlagTriggerViewModel> CreateFlagTriggerAsync(FeatureFlagTriggerViewModel trigger)
        {
            var newTrigger = new FeatureFlagTrigger()
            {
                Times = 0,
                Status = (int)FeatureFlagTriggerStatusEnum.Enabled,
                Type = (int)trigger.Type,
                Action = (int)trigger.Action,
                FeatureFlagId = trigger.FeatureFlagId,
                Description = trigger.Description,
                UpdatedAt = DateTime.UtcNow
            };

            newTrigger = await _mongoDbFeatureTriggerService.CreateAsync(newTrigger);
            var token = _jwtUtilsService.GenerateToken(newTrigger._Id);
            newTrigger.Token = token;
            newTrigger.Id = newTrigger._Id;
            newTrigger = await _mongoDbFeatureTriggerService.UpdateAsync(newTrigger._Id, newTrigger);
            trigger.Id = newTrigger._Id;
            trigger.UpdatedAt = newTrigger.UpdatedAt;
            trigger.Token = token;

            return trigger;
        }

        public async Task<FeatureFlagTrigger> DisableFlagTriggerAsync(string id, string featureFlagId)
        {
            var trigger = await _mongoDbFeatureTriggerService.GetByIdAndFeatureFlagIdAsync(id, featureFlagId);
            if (trigger != null)
            {
                trigger.Status = (int) FeatureFlagTriggerStatusEnum.Disabled;
                trigger.UpdatedAt = DateTime.UtcNow;
                return await _mongoDbFeatureTriggerService.UpdateAsync(id, trigger);
            }

            return null;
        }

        public async Task<FeatureFlagTrigger> EnableFlagTriggerAsync(string id, string featureFlagId)
        {
            var trigger = await _mongoDbFeatureTriggerService.GetByIdAndFeatureFlagIdAsync(id, featureFlagId);
            if (trigger != null)
            {
                trigger.Status = (int)FeatureFlagTriggerStatusEnum.Enabled;
                trigger.UpdatedAt = DateTime.UtcNow;
                await _mongoDbFeatureTriggerService.UpdateAsync(id, trigger);
                return trigger;
            }

            return null;
        }

        public async Task<FeatureFlagTrigger> ArchiveFlagTriggerAsync(string id, string featureFlagId)
        {
            var trigger = await _mongoDbFeatureTriggerService.GetByIdAndFeatureFlagIdAsync(id, featureFlagId);
            if (trigger != null)
            {
                trigger.Status = (int)FeatureFlagTriggerStatusEnum.Archived;
                trigger.UpdatedAt = DateTime.UtcNow;
                await _mongoDbFeatureTriggerService.UpdateAsync(id, trigger);
                return trigger;
            }

            return null;
        }

        public async Task<FeatureFlagTrigger> ResetFlagTriggerTokenAsync(string id, string featureFlagId)
        {
            var trigger = await _mongoDbFeatureTriggerService.GetByIdAndFeatureFlagIdAsync(id, featureFlagId);
            if (trigger != null)
            {
                trigger.Token = _jwtUtilsService.GenerateToken(trigger._Id);
                trigger.UpdatedAt = DateTime.UtcNow;
                await _mongoDbFeatureTriggerService.UpdateAsync(id, trigger);
                return trigger;
            }

            return null;
        }

        public async Task<List<PrequisiteFeatureFlagViewModel>> SearchPrequisiteFeatureFlagsAsync(int environmentId,
            string searchText = "", int pageIndex = 0, int pageSize = 20)
        {
            var returnResult = new List<PrequisiteFeatureFlagViewModel>();
            var ffs = await _mongoFeatureFlagsService.SearchAsync(searchText, environmentId, pageIndex, pageSize);
            foreach (var ff in ffs)
            {
                returnResult.Add(new PrequisiteFeatureFlagViewModel()
                {
                    EnvironmentId = environmentId,
                    Id = ff.Id,
                    KeyName = ff.FF.KeyName,
                    Name = ff.FF.Name,
                    VariationOptions = ff.VariationOptions
                });
            }

            return returnResult;
        }
    }
}