using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using Microsoft.Azure.Cosmos;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbService: INoSqlService
    {
        private readonly MongoDbFeatureFlagService _mongoFeatureFlagsService;
        private readonly MongoDbEnvironmentUserService _mongoEnvironmentUsersService;
        private readonly MongoDbEnvironmentUserPropertyService _mongoEnvironmentUserPropertiesService;

        public MongoDbService(
            MongoDbFeatureFlagService mongoFeatureFlagsService,
            MongoDbEnvironmentUserService mongoEnvironmentUsersService,
            MongoDbEnvironmentUserPropertyService mongoEnvironmentUserPropertiesService)
        {
            _mongoFeatureFlagsService = mongoFeatureFlagsService;
            _mongoEnvironmentUsersService = mongoEnvironmentUsersService;
            _mongoEnvironmentUserPropertiesService = mongoEnvironmentUserPropertiesService;
        }


        #region old version true false status functions
        public async Task TrueFalseStatusUpdateItemAsync(string id, dynamic item)
        {
        }
        public async Task<dynamic> TrueFalseStatusGetItemAsync(string id)
        {
            return null;
        }
        public async Task<EnvironmentFeatureFlagUser> TrueFalseStatusAddEnvironmentFeatureFlagUserAsync(EnvironmentFeatureFlagUser item)
        {
            return null;
        }
        public async Task<EnvironmentFeatureFlagUser> TrueFalseStatusGetEnvironmentFeatureFlagUserAsync(string id)
        {
            return null;
        }
        public async Task<int> TrueFalseStatusGetFeatureFlagTotalUsersAsync(string featureFlagId)
        {
            return 0;
        }
        public async Task<int> TrueFalseStatusGetFeatureFlagHitUsersAsync(string featureFlagId)
        {
            return 0;
        }
        #endregion


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
            catch (MongoException ex)
            {
                return null;
            }
        }

        public async Task<FeatureFlag> CreateFeatureFlagAsync(CreateFeatureFlagViewModel param, string currentUserId, int projectId, int accountId)
        {
            var keyName = FeatureFlagKeyExtension.CreateNewFeatureFlagKeyName(param.EnvironmentId, param.Name);
            var featureFlagId = FeatureFlagKeyExtension.GetFeatureFlagId(keyName, param.EnvironmentId.ToString(), accountId.ToString(), projectId.ToString());
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
                            RolloutPercentage = new double[2]{ 0, 1},
                            ValueOption = new VariationOption() {
                                DisplayOrder = 1,
                                LocalId = 1,
                                VariationValue = "true"
                            }
                        }
                    }
                },
                FFP = new List<FeatureFlagPrerequisite>(),
                FFTIUForFalse = new List<FeatureFlagTargetIndividualUser>(),
                FFTIUForTrue = new List<FeatureFlagTargetIndividualUser>(),
                FFTUWMTR = new List<FeatureFlagTargetUsersWhoMatchTheseRuleParam>(),
                VariationOptions = new List<VariationOption>() {
                    new VariationOption() {
                        DisplayOrder = 1,
                        LocalId = 1,
                        VariationValue = "true"
                    },
                    new VariationOption() {
                        DisplayOrder = 2,
                        LocalId = 2,
                        VariationValue = "false"
                    },
                },
                IsMultiOptionMode = true,
                TargetIndividuals = new List<TargetIndividualForVariationOption>()
            };
            return await _mongoFeatureFlagsService.CreateAsync(newFeatureFlag);
        }


        public async Task<ReturnJsonModel<FeatureFlag>> UpdateMultiValueOptionSupportedFeatureFlagAsync(FeatureFlag param)
        {
            try
            {
                if (param.FF.DefaultRulePercentageRollouts == null || param.FF.DefaultRulePercentageRollouts.Count == 0)
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, DefaultRulePercentageRollouts shouldn't be empty")
                    };
                if (param.FF.VariationOptionWhenDisabled == null)
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, MultiOptionValueWhenDisabled shouldn't be empty")
                    };
                if (param.FFP != null && param.FFP.Count > 0 && param.FFP.Any(p => p.ValueOptionsVariationValue == null))
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, all ValueOptionsVariationValue FFPs shouldn't be empty")
                    };
                if (param.FFTUWMTR != null && param.FFTUWMTR.Count > 0 && param.FFTUWMTR.Any(p => p.ValueOptionsVariationRuleValues == null || p.ValueOptionsVariationRuleValues.Count == 0))
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, ValueOptionsVariationRuleValues in all FFTUWMTRs shouldn't be empty")
                    };
                if (param.VariationOptions == null || param.VariationOptions.Count == 0)
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, VariationOptions shouldn't be empty")
                    };
                if (param.TargetIndividuals != null && param.TargetIndividuals.Count >= 0 && param.TargetIndividuals.Any(p => p.ValueOption == null))
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, ValueOption in TargetIndividual shouldn't be empty")
                    };

                param.EnvironmentId = param.FF.EnvironmentId;
                param.Id = param.FF.Id;
                param.FF.LastUpdatedTime = DateTime.UtcNow;
                param.FF.DefaultRuleValue = null;
                param.FF.ValueWhenDisabled = null;


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
                    else
                    {
                        var fftu = originFF.FFTUWMTR.FirstOrDefault(p => p.RuleId == item.RuleId);
                        if(fftu != null)
                        {
                            item.PercentageRolloutForFalseNumber = originFF.FFTUWMTR.FirstOrDefault(p => p.RuleId == item.RuleId).PercentageRolloutForFalseNumber;
                            item.PercentageRolloutForTrueNumber = originFF.FFTUWMTR.FirstOrDefault(p => p.RuleId == item.RuleId).PercentageRolloutForTrueNumber;
                        }
                    }
                }
            }
            if (originFF.FF.PercentageRolloutForFalse != null && originFF.FF.PercentageRolloutForTrue != null &&
                param.FF.PercentageRolloutForFalse != null && param.FF.PercentageRolloutForTrue != null)
            {
                param.FF.PercentageRolloutForFalseNumber = originFF.FF.PercentageRolloutForFalseNumber;
                param.FF.PercentageRolloutForTrueNumber = originFF.FF.PercentageRolloutForTrueNumber;
            }

            param._Id = originFF._Id;
            await _mongoFeatureFlagsService.UpdateAsync(param.Id, param);

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

        public async Task<EnvironmentUserProperty> UpdateEnvironmentUserPropertiesAsync(int environmentId, List<string> propertyName)
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
                await _mongoEnvironmentUserPropertiesService.UpdateAsync(environmentUserProperty.Id, environmentUserProperty);
            }
            catch (MongoException ex)
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
            catch (MongoException ex)
            {
                return new EnvironmentUserProperty() { 
                    Properties = new List<string>()
                };
            }
        }


        public async Task<FeatureFlag> GetFlagAsync(string id)
        {
            return await _mongoFeatureFlagsService.GetAsync(id);
        }

        public async Task<List<FeatureFlagBasicInfo>> GetEnvironmentFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100)
        {
            var returnResult = new List<FeatureFlagBasicInfo>();
            var ffs = (await _mongoFeatureFlagsService.SearchAsync(null, environmentId, pageIndex, pageSize));
            foreach (var ff in ffs)
            {
                var ffb = ff.FF;
                if (ffb != null)
                {
                    ffb.Status = string.IsNullOrWhiteSpace(ffb.Status) ? FeatureFlagStatutEnum.Enabled.ToString() : ffb.Status;
                    returnResult.Add(ffb);
                }
            }
            return returnResult;
        }

        public async Task<List<FeatureFlagBasicInfo>> GetEnvironmentArchivedFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100)
        {
            var returnResult = new List<FeatureFlagBasicInfo>();
            var ffs = (await _mongoFeatureFlagsService.SearchArchivedAsync(environmentId, pageIndex, pageSize));
            foreach (var ff in ffs)
            {
                var ffb = ff.FF;
                if (ffb != null)
                {
                    ffb.Status = string.IsNullOrWhiteSpace(ffb.Status) ? FeatureFlagStatutEnum.Enabled.ToString() : ffb.Status;
                    returnResult.Add(ffb);
                }
            }
            return returnResult;
        }



        public async Task<int> QueryEnvironmentUsersCountAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            return await _mongoEnvironmentUsersService.CountAsync(searchText, environmentId);
        }



        public async Task<List<EnvironmentUser>> QueryEnvironmentUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            return await _mongoEnvironmentUsersService.SearchAsync(searchText, environmentId, pageIndex, pageSize);
        }

        public async Task<EnvironmentUserProperty> GetEnvironmentUserPropertiesForCRUDAsync(int environmentId)
        {
            var eup = await _mongoEnvironmentUserPropertiesService.GetByEnvironmentIdAsync(environmentId);
            if (eup == null)
                eup = new EnvironmentUserProperty
                {
                    EnvironmentId = environmentId,
                    Properties = new List<string>()
                };
            return eup;
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


        public async Task<List<PrequisiteFeatureFlagViewModel>> SearchPrequisiteFeatureFlagsAsync(int environmentId, string searchText = "", int pageIndex = 0, int pageSize = 20)
        {
            var returnResult = new List<PrequisiteFeatureFlagViewModel>();
            var ffs = await _mongoFeatureFlagsService.SearchAsync(searchText, environmentId, pageIndex, pageSize);
            foreach(var ff in ffs)
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
