using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public interface ICosmosDbService
    {
        Task<List<CosmosDBEnvironmentUser>> QueryEnvironmentUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize);
        Task<int> QueryEnvironmentUsersCountAsync(string searchText, int environmentId, int pageIndex, int pageSize);
        Task<dynamic> GetItemByKeyIdAsync(string keyId);
        Task<dynamic> GetItemAsync(string id);
        Task<CosmosDBEnvironmentUser> GetEnvironmentUserAsync(string id);
        Task<CosmosDBEnvironmentFeatureFlagUser> GetEnvironmentFeatureFlagUserAsync(string id);
        Task<CosmosDBFeatureFlag> GetFeatureFlagAsync(string id);
        Task<dynamic> AddItemAsync(dynamic item);
        Task<CosmosDBEnvironmentUser> AddCosmosDBEnvironmentUserAsync(CosmosDBEnvironmentUser item);
        Task<CosmosDBEnvironmentFeatureFlagUser> AddCosmosDBEnvironmentFeatureFlagUserAsync(CosmosDBEnvironmentFeatureFlagUser item);
        Task UpdateItemAsync(string id, dynamic item);
        Task DeleteItemAsync(string id);
        Task<List<CosmosDBFeatureFlagBasicInfo>> GetEnvironmentFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100);
        Task<List<CosmosDBFeatureFlagBasicInfo>> GetEnvironmentArchivedFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100);

        Task<CosmosDBFeatureFlag> GetCosmosDBFeatureFlagAsync(string id);
        Task<CosmosDBEnvironmentUserProperty> GetCosmosDBEnvironmentUserPropertiesForCRUDAsync(int environmentId);
        Task CreateOrUpdateCosmosDBEnvironmentUserPropertiesForCRUDAsync(CosmosDBEnvironmentUserProperty param);

        Task<CosmosDBFeatureFlag> CreateCosmosDBFeatureFlagAsync(CreateFeatureFlagViewModel param, string currentUserId, int projectId, int accountId);
        Task<CosmosDBFeatureFlag> UpdateCosmosDBFeatureFlagAsync(CosmosDBFeatureFlag param);
        Task<ReturnJsonModel<CosmosDBFeatureFlag>> UpdateMultiValueOptionSupportedFeatureFlagAsync(CosmosDBFeatureFlag param);
        Task<CosmosDBFeatureFlag> ArchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param);
        Task<CosmosDBFeatureFlag> UnarchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param);

        Task<CosmosDBEnvironmentUserProperty> UpdateCosmosDBEnvironmentUserPropertiesAsync(int environmentId, List<string> propertyName);
        Task<CosmosDBEnvironmentUserProperty> GetCosmosDBEnvironmentUserPropertiesAsync(int environmentId);

        Task UpsertEnvironmentUserAsync(CosmosDBEnvironmentUser param);

        Task<int> GetFeatureFlagTotalUsersAsync(string featureFlagId);
        Task<int> GetFeatureFlagHitUsersAsync(string featureFlagId);


        Task CreateEnvrionmentUserForPerformanceTest();
    }

    public class CosmosDbService: ICosmosDbService
    {
        private Container _container;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task<dynamic> AddItemAsync(dynamic item)
        {
            var newItem =  await this._container.CreateItemAsync<dynamic>(item, new PartitionKey(item.id), new ItemRequestOptions());
            return newItem;
        }

        public async Task<CosmosDBEnvironmentUser> AddCosmosDBEnvironmentUserAsync(CosmosDBEnvironmentUser item)
        {
            var newItem = await this._container.CreateItemAsync<CosmosDBEnvironmentUser>(item, new PartitionKey(item.id), new ItemRequestOptions());
            return newItem;
        }

        public async Task<CosmosDBEnvironmentFeatureFlagUser> AddCosmosDBEnvironmentFeatureFlagUserAsync(CosmosDBEnvironmentFeatureFlagUser item)
        {
            var newItem = await this._container.CreateItemAsync<CosmosDBEnvironmentFeatureFlagUser>(item, new PartitionKey(item.id), new ItemRequestOptions());
            return newItem;
        }

        

        public async Task DeleteItemAsync(string id)
        {
            await this._container.DeleteItemAsync<dynamic>(id, new PartitionKey(id));
        }

        
        public async Task<dynamic> GetItemByKeyIdAsync(string keyId)
        {
            var query = this._container.GetItemQueryIterator<dynamic>(
                "SELECT * " +
                "FROM f " +
                $"where f.keyId = \"{keyId}\"");
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                return response.ToList()[0];
            }
            return null;
        }
        public async Task<dynamic> GetItemAsync(string id)
        {
            try
            {
                return await this._container.ReadItemAsync<dynamic>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<CosmosDBFeatureFlag> GetFeatureFlagAsync(string id)
        {
            return await this._container.ReadItemAsync<CosmosDBFeatureFlag>(id, new PartitionKey(id));
        }
        public async Task<CosmosDBEnvironmentUser> GetEnvironmentUserAsync(string id)
        {
            try
            {
                return await this._container.ReadItemAsync<CosmosDBEnvironmentUser>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
        public async Task<CosmosDBEnvironmentFeatureFlagUser> GetEnvironmentFeatureFlagUserAsync(string id)
        {
            try
            {
                return await this._container.ReadItemAsync<CosmosDBEnvironmentFeatureFlagUser>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task UpdateItemAsync(string id, dynamic item)
        {
            await this._container.UpsertItemAsync<dynamic>(item, new PartitionKey(id));
        }


        public async Task<CosmosDBFeatureFlag> CreateCosmosDBFeatureFlagAsync(CreateFeatureFlagViewModel param, string currentUserId, int projectId, int accountId)
        {
            var keyName = FeatureFlagKeyExtension.CreateNewFeatureFlagKeyName(param.EnvironmentId, param.Name);
            var featureFlagId = FeatureFlagKeyExtension.GetFeatureFlagId(keyName, param.EnvironmentId.ToString(), accountId.ToString(), projectId.ToString());
            var newFeatureFlag = new CosmosDBFeatureFlag()
            {
                Id = featureFlagId,
                EnvironmentId = param.EnvironmentId,
                FF = new CosmosDBFeatureFlagBasicInfo
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
                            RolloutPercentage = new double[2]{ 0, 100},
                            ValueOption = new VariationOption() {
                                DisplayOrder = 1,
                                LocalId = 1,
                                VariationValue = "true"
                            }
                        }
                    }
                },
                FFP = new List<CosmosDBFeatureFlagPrerequisite>(),
                FFTIUForFalse = new List<CosmosDBFeatureFlagTargetIndividualUser>(),
                FFTIUForTrue = new List<CosmosDBFeatureFlagTargetIndividualUser>(),
                FFTUWMTR = new List<CosmosDBFeatureFlagTargetUsersWhoMatchTheseRuleParam>(),
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
            return await _container.CreateItemAsync<CosmosDBFeatureFlag>(newFeatureFlag);
        }


        public async Task<ReturnJsonModel<CosmosDBFeatureFlag>> UpdateMultiValueOptionSupportedFeatureFlagAsync(CosmosDBFeatureFlag param)
        {
            try
            {
                if (param.FF.DefaultRulePercentageRollouts == null || param.FF.DefaultRulePercentageRollouts.Count == 0)
                    return new ReturnJsonModel<CosmosDBFeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, DefaultRulePercentageRollouts shouldn't be empty")
                    };
                if (param.FF.VariationOptionWhenDisabled == null)
                    return new ReturnJsonModel<CosmosDBFeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, MultiOptionValueWhenDisabled shouldn't be empty")
                    };
                if (param.FFP != null && param.FFP.Count > 0 && param.FFP.Any(p => p.ValueOptionsVariationValue == null))
                    return new ReturnJsonModel<CosmosDBFeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, all ValueOptionsVariationValue FFPs shouldn't be empty")
                    };
                if (param.FFTUWMTR != null && param.FFTUWMTR.Count > 0 && param.FFTUWMTR.Any(p => p.ValueOptionsVariationRuleValues == null || p.ValueOptionsVariationRuleValues.Count == 0))
                    return new ReturnJsonModel<CosmosDBFeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, ValueOptionsVariationRuleValues in all FFTUWMTRs shouldn't be empty")
                    };
                if (param.VariationOptions == null || param.VariationOptions.Count == 0)
                    return new ReturnJsonModel<CosmosDBFeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, VariationOptions shouldn't be empty")
                    };
                if (param.TargetIndividuals != null && param.TargetIndividuals.Count >= 0 && param.TargetIndividuals.Any(p => p.ValueOption == null))
                    return new ReturnJsonModel<CosmosDBFeatureFlag>()
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

                await this._container.UpsertItemAsync<CosmosDBFeatureFlag>(param);

                return new ReturnJsonModel<CosmosDBFeatureFlag>
                {
                    StatusCode = 200,
                    Data = param
                };
            }
            catch (Exception exp)
            {
                return new ReturnJsonModel<CosmosDBFeatureFlag>
                {
                    StatusCode = 500,
                    Error = exp
                };
            }

        }

        public async Task<CosmosDBFeatureFlag> UpdateCosmosDBFeatureFlagAsync(CosmosDBFeatureFlag param)
        {
            var originFF = await this.GetCosmosDBFeatureFlagAsync(param.Id);
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
            return await this._container.UpsertItemAsync<CosmosDBFeatureFlag>(param);
        }

        public async Task<CosmosDBFeatureFlag> ArchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param)
        {
            var originFF = await this.GetCosmosDBFeatureFlagAsync(param.FeatureFlagId);
            originFF.FF.LastUpdatedTime = DateTime.UtcNow;
            originFF.IsArchived = true;
            originFF.FF.Status = FeatureFlagStatutEnum.Disabled.ToString();

            return await this._container.UpsertItemAsync<CosmosDBFeatureFlag>(originFF);
        }

        public async Task<CosmosDBFeatureFlag> UnarchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param)
        {
            var originFF = await this.GetCosmosDBFeatureFlagAsync(param.FeatureFlagId);
            originFF.FF.LastUpdatedTime = DateTime.UtcNow;
            originFF.IsArchived = false;

            return await this._container.UpsertItemAsync<CosmosDBFeatureFlag>(originFF);
        }

        public async Task<CosmosDBEnvironmentUserProperty> UpdateCosmosDBEnvironmentUserPropertiesAsync(int environmentId, List<string> propertyName)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(environmentId);
            CosmosDBEnvironmentUserProperty environmentUserProperty = null;
            try
            {
                environmentUserProperty = await this._container.ReadItemAsync<CosmosDBEnvironmentUserProperty>(id, new PartitionKey(id));
                if (propertyName != null && propertyName.Count > 0)
                {
                    foreach (var name in propertyName)
                    {
                        environmentUserProperty.Properties.Add(name);
                    }
                }
                environmentUserProperty.Properties = environmentUserProperty.Properties.Distinct().ToList();
                environmentUserProperty = await this._container.UpsertItemAsync<CosmosDBEnvironmentUserProperty>(environmentUserProperty);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                environmentUserProperty = (await this._container.CreateItemAsync<CosmosDBEnvironmentUserProperty>(
                    new CosmosDBEnvironmentUserProperty
                    {
                        Id = id,
                        Properties = propertyName ?? new List<string>(),
                        EnvironmentId = environmentId
                    }));
            }
            return environmentUserProperty;
        }

        public async Task<CosmosDBEnvironmentUserProperty> GetCosmosDBEnvironmentUserPropertiesAsync(int environmentId)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(environmentId);
            try
            {
                CosmosDBEnvironmentUserProperty returnModel = await this._container.ReadItemAsync<CosmosDBEnvironmentUserProperty>(id, new PartitionKey(id));
                returnModel.Properties.Add("KeyId");
                returnModel.Properties.Add("Name");
                returnModel.Properties.Add("Email");
                return returnModel;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new CosmosDBEnvironmentUserProperty()
                {
                    Properties = new List<string>() { "KeyId", "Name", "Email" }
                };
            }
        }


        public async Task<CosmosDBFeatureFlag> GetCosmosDBFeatureFlagAsync(string id)
        {
            return await this._container.ReadItemAsync<CosmosDBFeatureFlag>(id, new PartitionKey(id));
        }

        public async Task<List<CosmosDBFeatureFlagBasicInfo>> GetEnvironmentFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100)
        {
            var returnResult = new List<CosmosDBFeatureFlagBasicInfo>();
                var results = new List<CosmosDBFeatureFlag>();
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
                        CosmosDBFeatureFlagBasicInfo ff = item.ToObject<CosmosDBFeatureFlag>().FF;
                        if (string.IsNullOrWhiteSpace(ff.Status))
                            ff.Status = FeatureFlagStatutEnum.Enabled.ToString();
                        returnResult.Add(ff);
                    }
                }
            }
            return returnResult;
        }

        public async Task<List<CosmosDBFeatureFlagBasicInfo>> GetEnvironmentArchivedFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100)
        {
            var returnResult = new List<CosmosDBFeatureFlagBasicInfo>();
            var results = new List<CosmosDBFeatureFlag>();
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
                        returnResult.Add(item.ToObject<CosmosDBFeatureFlag>().FF);
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
                QueryDefinition queryDefinition = new QueryDefinition("select value count(1) from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentUser' and (f.Name like '%" + searchText + "%' or f.KeyId like '%" + searchText + "')")
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



        public async Task<List<CosmosDBEnvironmentUser>> QueryEnvironmentUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            List<CosmosDBEnvironmentUser> returnResult = new List<CosmosDBEnvironmentUser>();
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
                            returnResult.Add(item.ToObject<CosmosDBEnvironmentUser>());
                        }
                    }
                }
                return returnResult;
            }
            else
            {
                QueryDefinition queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentUser' and (f.Name like '%" + searchText + "%' or f.KeyId like '%" + searchText + "')")
                    .WithParameter("@environmentId", environmentId)
                    .WithParameter("@searchText", searchText);
                using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                        foreach (var item in response)
                        {
                            returnResult.Add(item.ToObject<CosmosDBEnvironmentUser>());
                        }
                    }
                }
                return returnResult;
            }
        }

        public async Task<CosmosDBEnvironmentUserProperty> GetCosmosDBEnvironmentUserPropertiesForCRUDAsync(int environmentId)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(environmentId);
            try
            {
                return await this._container.ReadItemAsync<CosmosDBEnvironmentUserProperty>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new CosmosDBEnvironmentUserProperty()
                {
                    EnvironmentId = environmentId,
                    Id = id,
                    Properties = new List<string>()
                };
            }
        }

        public async Task CreateOrUpdateCosmosDBEnvironmentUserPropertiesForCRUDAsync(CosmosDBEnvironmentUserProperty param)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(param.EnvironmentId);
            await this._container.UpsertItemAsync<CosmosDBEnvironmentUserProperty>(new CosmosDBEnvironmentUserProperty
            {
                Id = id,
                EnvironmentId = param.EnvironmentId,
                Properties = param.Properties,
                ObjectType = param.ObjectType
            });
        }

        public async Task UpsertEnvironmentUserAsync(CosmosDBEnvironmentUser param)
        {
            await this._container.UpsertItemAsync<CosmosDBEnvironmentUser>(param);
        }

        public async Task<int> GetFeatureFlagTotalUsersAsync(string featureFlagId)
        {
            int envId = FeatureFlagKeyExtension.GetEnvIdByFeautreFlagId(featureFlagId);
            QueryDefinition queryDefinition = new QueryDefinition("select value count(1) from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentFFUser'")
              .WithParameter("@environmentId", envId);
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
            return 0;
        }


        public async Task<int> GetFeatureFlagHitUsersAsync(string featureFlagId)
        {
            int envId = FeatureFlagKeyExtension.GetEnvIdByFeautreFlagId(featureFlagId);
            QueryDefinition queryDefinition = new QueryDefinition("select value count(1) from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentFFUser' and f.ResultValue = true")
              .WithParameter("@environmentId", envId);
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
            return 0;
        }

        public async Task CreateEnvrionmentUserForPerformanceTest()
        {
            var ffs = new List<string>();
            for (int i = 0; i < 30; i++)
            {
                ffs.Add("FeatureFlagKey" + i.ToString() + "@@--@@" + "VariationValueCanBeVeryVeryLongLongLongLongVariationValueCanBeVeryVeryLongLongLongLongVariationValueCanBeVeryVeryLongLongLongLong");
            }
            for (int i = 0; i < 2000;i++)
            {
                await UpsertEnvironmentUserAsync(new CosmosDBEnvironmentUser()
                {
                    id = Guid.NewGuid().ToString(),
                    KeyId = "Performance Test " + Guid.NewGuid().ToString(),
                    CustomizedProperties = new List<FeatureFlagUserCustomizedProperty> {
                             new FeatureFlagUserCustomizedProperty(){ Name = "123", Value = "123" },
                             new FeatureFlagUserCustomizedProperty(){ Name = "1111111", Value = "333333" },
                             new FeatureFlagUserCustomizedProperty(){ Name = "22222", Value = "444444" },
                             new FeatureFlagUserCustomizedProperty(){ Name = "33333", Value = "55555" },
                         },
                    Name = "Performance Test " + Guid.NewGuid().ToString(),
                    ObjectType = "EnvironmentUser",
                    EnvironmentId = 7,
                    FeatureFlags = ffs
                });
            }
        }
    }
}
