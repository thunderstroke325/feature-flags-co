using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.DataSync;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.ViewModels.FeatureFlagTrigger;

namespace FeatureFlags.APIs.Services
{
    class CosmosDbCountResult 
    {
        int Count { get; set; }
    }

    public class CosmosDbService
    {
        private Container _container;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task<EnvironmentUser> AddEnvironmentUserAsync(EnvironmentUser item)
        {
            var newItem = await this._container.CreateItemAsync<EnvironmentUser>(item, new PartitionKey(item.Id), new ItemRequestOptions());
            return newItem;
        }


        public async Task<FeatureFlag> GetFeatureFlagAsync(string id)
        {
            return await this._container.ReadItemAsync<FeatureFlag>(id, new PartitionKey(id));
        }
        public async Task<EnvironmentUser> GetEnvironmentUserAsync(string id)
        {
            try
            {
                return await this._container.ReadItemAsync<EnvironmentUser>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
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
                TargetIndividuals = new List<TargetIndividualForVariationOption>()
            };
            return await _container.CreateItemAsync<FeatureFlag>(newFeatureFlag);
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

                await this._container.UpsertItemAsync<FeatureFlag>(param);

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

            return await this._container.UpsertItemAsync<FeatureFlag>(param);
        }

        public async Task<FeatureFlag> ArchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param)
        {
            var originFF = await this.GetFlagAsync(param.FeatureFlagId);
            originFF.FF.LastUpdatedTime = DateTime.UtcNow;
            originFF.IsArchived = true;
            originFF.FF.Status = FeatureFlagStatutEnum.Disabled.ToString();

            return await this._container.UpsertItemAsync<FeatureFlag>(originFF);
        }

        public async Task<FeatureFlag> UnarchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param)
        {
            var originFF = await this.GetFlagAsync(param.FeatureFlagId);
            originFF.FF.LastUpdatedTime = DateTime.UtcNow;
            originFF.IsArchived = false;

            return await this._container.UpsertItemAsync<FeatureFlag>(originFF);
        }

        public async Task<EnvironmentUserProperty> UpdateEnvironmentUserPropertiesAsync(int environmentId, List<string> propertyName)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(environmentId);
            EnvironmentUserProperty environmentUserProperty = null;
            try
            {
                environmentUserProperty = await this._container.ReadItemAsync<EnvironmentUserProperty>(id, new PartitionKey(id));
                if (propertyName != null && propertyName.Count > 0)
                {
                    foreach (var name in propertyName)
                    {
                        environmentUserProperty.Properties.Add(name);
                    }
                }
                environmentUserProperty.Properties = environmentUserProperty.Properties.Distinct().ToList();
                environmentUserProperty = await this._container.UpsertItemAsync<EnvironmentUserProperty>(environmentUserProperty);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                environmentUserProperty = (await this._container.CreateItemAsync<EnvironmentUserProperty>(
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
                EnvironmentUserProperty returnModel = await this._container.ReadItemAsync<EnvironmentUserProperty>(id, new PartitionKey(id));
                returnModel.Properties.Add("KeyId");
                returnModel.Properties.Add("Name");
                returnModel.Properties.Add("Email");
                return returnModel;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new EnvironmentUserProperty()
                {
                    Properties = new List<string>() { "KeyId", "Name", "Email" }
                };
            }
        }


        public async Task<FeatureFlag> GetFlagAsync(string id)
        {
            return await this._container.ReadItemAsync<FeatureFlag>(id, new PartitionKey(id));
        }

        private async Task<int> GetDataCount(int envId, string objectType)
        {
            int totalCount = 0;

            QueryDefinition queryDefinition = new QueryDefinition("select value count(1) from f where f.EnvironmentId = @envId and f.ObjectType = @objectType")
                .WithParameter("@envId", envId)
                .WithParameter("@objectType", objectType);

            using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
            {
                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        totalCount = (int)item;
                    }
                }
            }

            return totalCount;
        }

        public async Task<List<T>> GetEnvironmentDataAsync<T>(int envId)
        {
            var result = new List<T>();
            int pageSize = 1000;
            string objectType = string.Empty;

            if (typeof(T) == typeof(FeatureFlag))
            {
                objectType = "FeatureFlag";
            }
            else if (typeof(T) == typeof(EnvironmentUser))
            {
                objectType = "EnvironmentUser";
            }
            else if (typeof(T) == typeof(EnvironmentUserProperty)) {
                objectType = "EnvironmentUserProperties";
            }

            int totalCount = await GetDataCount(envId, objectType);

            for (int pageIndex = 0; pageIndex <= totalCount / pageSize; pageIndex++)
            {
                QueryDefinition queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @envId and f.ObjectType = @objectType offset @offsetNumber limit @pageSize")
                    .WithParameter("@envId", envId)
                    .WithParameter("@objectType", objectType)
                    .WithParameter("@offsetNumber", pageIndex * pageSize)
                    .WithParameter("@pageSize", pageSize);

                using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                        foreach (var item in response)
                        {
                            result.Add(item.ToObject<T>());
                        }
                    }
                }
            }

            return result;
        }

        public async Task SaveEnvironmentDataAsync(int accountId, int projectId, int envId, EnvironmentDataViewModel data) 
        {
            data.FeatureFlags.ForEach(async ff => {
                ff.EnvironmentId = envId;
                await this._container.UpsertItemAsync(ff); 
            });

            data.EnvironmentUsers.ForEach(async u => {
                u.EnvironmentId = envId;
                await this._container.UpsertItemAsync(u);
            });

            if (data.EnvironmentUserProperties != null)
            {
                data.EnvironmentUserProperties.EnvironmentId = envId;
                await this._container.UpsertItemAsync(data.EnvironmentUserProperties);
            }

            // TODO refresh cache
        }

        public async Task<List<FeatureFlagBasicInfo>> GetEnvironmentFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100)
        {
            var returnResult = new List<FeatureFlagBasicInfo>();
                
            QueryDefinition queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.IsArchived != true and f.ObjectType = 'FeatureFlag' offset @offsetNumber limit @pageSize")
                .WithParameter("@environmentId", environmentId)
                .WithParameter("@offsetNumber", pageIndex * pageSize)
                .WithParameter("@pageSize", pageSize);
            //using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>("select * from f where f.EnvironmentId = 1 and f.ObjectType = 'FeatureFlag'"))
            using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
            {
                while (feedIterator.HasMoreResults)
                {
                    Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        FeatureFlagBasicInfo ff = item.ToObject<FeatureFlag>().FF;
                        if (string.IsNullOrWhiteSpace(ff.Status))
                            ff.Status = FeatureFlagStatutEnum.Enabled.ToString();
                        returnResult.Add(ff);
                    }
                }
            }
            return returnResult;
        }

        public async Task<List<FeatureFlagBasicInfo>> GetEnvironmentArchivedFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100)
        {
            var returnResult = new List<FeatureFlagBasicInfo>();
            var results = new List<FeatureFlag>();
            QueryDefinition queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.IsArchived = true and f.ObjectType = 'FeatureFlag' offset @offsetNumber limit @pageSize")
                .WithParameter("@environmentId", environmentId)
                .WithParameter("@offsetNumber", pageIndex * pageSize)
                .WithParameter("@pageSize", pageSize);
            //using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>("select * from f where f.EnvironmentId = 1 and f.ObjectType = 'FeatureFlag'"))
            using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
            {
                while (feedIterator.HasMoreResults)
                {
                    Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        returnResult.Add(item.ToObject<FeatureFlag>().FF);
                    }
                }
            }
            return returnResult;
        }


        public async Task<int> QueryEnvironmentUsersCountAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace((searchText ?? "").Trim()))
            {
                QueryDefinition queryDefinition = new QueryDefinition("select value count(1) from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentUser'")
                  .WithParameter("@environmentId", environmentId);
                using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                        foreach (var item in response)
                        {
                            return (int)item;
                        }
                    }
                }
            }
            else
            {
                QueryDefinition queryDefinition = new QueryDefinition("select value count(1) from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentUser' and (f.Name like '%@searchText%' or f.KeyId like '%@searchText%')")
                    .WithParameter("@environmentId", environmentId)
                    .WithParameter("@searchText", searchText);
                using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                        foreach (var item in response)
                        {
                            return (int)item;
                        }
                    }
                }
            }

            return 0;
        }



        public async Task<List<EnvironmentUser>> QueryEnvironmentUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            List<EnvironmentUser> returnResult = new List<EnvironmentUser>();
            if (string.IsNullOrWhiteSpace((searchText ?? "").Trim()))
            {
                QueryDefinition queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentUser' offset @offsetNumber limit @pageSize")
                  .WithParameter("@environmentId", environmentId)
                  .WithParameter("@offsetNumber", pageIndex * pageSize)
                  .WithParameter("@pageSize", pageSize);
                using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                        foreach (var item in response)
                        {
                            returnResult.Add(item.ToObject<EnvironmentUser>());
                        }
                    }
                }
                return returnResult;
            }
            else
            {
                QueryDefinition queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentUser' and (f.Name like '%@searchText%' or f.KeyId like '%@searchText%')")
                    .WithParameter("@environmentId", environmentId)
                    .WithParameter("@searchText", searchText);
                using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                        foreach (var item in response)
                        {
                            returnResult.Add(item.ToObject<EnvironmentUser>());
                        }
                    }
                }
                return returnResult;
            }
        }

        public async Task<EnvironmentUserProperty> GetEnvironmentUserPropertiesForCRUDAsync(int environmentId)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(environmentId);
            try
            {
                return await this._container.ReadItemAsync<EnvironmentUserProperty>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new EnvironmentUserProperty()
                {
                    EnvironmentId = environmentId,
                    Id = id,
                    Properties = new List<string>()
                };
            }
        }

        public async Task CreateOrUpdateEnvironmentUserPropertiesForCRUDAsync(EnvironmentUserProperty param)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(param.EnvironmentId);
            await this._container.UpsertItemAsync<EnvironmentUserProperty>(new EnvironmentUserProperty
            {
                Id = id,
                EnvironmentId = param.EnvironmentId,
                Properties = param.Properties,
                ObjectType = param.ObjectType
            });
        }

        public async Task UpsertEnvironmentUserAsync(EnvironmentUser param)
        {
            await this._container.UpsertItemAsync<EnvironmentUser>(param);
        }


        public async Task<List<PrequisiteFeatureFlagViewModel>> SearchPrequisiteFeatureFlagsAsync(int environmentId, string searchText = "", int pageIndex = 0, int pageSize = 20)
        {

            var returnResult = new List<PrequisiteFeatureFlagViewModel>();
            QueryDefinition queryDefinition = null;
            if (string.IsNullOrWhiteSpace((searchText ?? "").Trim()))
            {
                queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.IsArchived != true and f.ObjectType = 'FeatureFlag' offset @offsetNumber limit @pageSize")
                    .WithParameter("@environmentId", environmentId)
                    .WithParameter("@offsetNumber", pageIndex * pageSize)
                    .WithParameter("@pageSize", pageSize);
               
            }
            else
            {
                queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.IsArchived != true and f.ObjectType = 'FeatureFlag' and f.FF.Name like '%@searchText%'  offset @offsetNumber limit @pageSize")
                    .WithParameter("@environmentId", environmentId)
                    .WithParameter("@offsetNumber", pageIndex * pageSize)
                    .WithParameter("@pageSize", pageSize)
                    .WithParameter("@searchText", searchText);
            }
            using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
            {
                while (feedIterator.HasMoreResults)
                {
                    Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        FeatureFlag ff = item.ToObject<FeatureFlag>();
                        returnResult.Add(new PrequisiteFeatureFlagViewModel()
                        {
                            EnvironmentId = environmentId,
                            Id = ff.Id,
                            KeyName = ff.FF.KeyName,
                            Name = ff.FF.Name,
                            VariationOptions = ff.VariationOptions
                        });
                    }
                }
            }


            return returnResult;
        }

        public Task<List<FeatureFlagTrigger>> GetFlagTriggersByFfIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task UpOrDownFeatureFlagByFlagTriggerAsync(string token)
        {
            throw new NotImplementedException();
        }

        public Task<FeatureFlagTriggerViewModel> CreateFlagTriggerAsync(FeatureFlagTriggerViewModel trigger)
        {
            throw new NotImplementedException();
        }

        public Task<FeatureFlagTrigger> DisableFlagTriggerAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<FeatureFlagTrigger> DeleteFlagTriggerAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<FeatureFlagTrigger> ResetFlagTriggerTokenAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
