using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Services
{
    public class FeatureFlagV2Service : ITransientDependency
    {
        private readonly MongoDbPersist _mongoDb;
        private readonly INoSqlService _mongoDbServiceV1;

        public FeatureFlagV2Service(
            MongoDbPersist mongoDb, 
            INoSqlService mongoDbServiceV1)
        {
            _mongoDb = mongoDb;
            _mongoDbServiceV1 = mongoDbServiceV1;
        }

        public async Task<FeatureFlag> GetAsync(string id)
        {
            var featureFlag = await _mongoDb
                .QueryableOf<FeatureFlag>()
                .FirstOrDefaultAsync(flag => flag.Id == id);

            return featureFlag;
        }

        public async Task<List<DropdownItem>> GetDropDownsAsync(int envId)
        {
            var query = _mongoDb.QueryableOf<FeatureFlag>()
                .Where(flag => flag.EnvironmentId == envId)
                .Select(flag => new DropdownItem
                {
                    Key = flag.Id,
                    Value = flag.FF.Name
                });

            var dropdowns = await query.ToListAsync();

            return dropdowns;
        }

        public async Task<PagedResult<FeatureFlagBasicInfo>> GetListAsync(
            int envId,
            string name,
            string status,
            IEnumerable<string> flagIds,
            bool includeArchived,
            int page,
            int pageSize)
        {
            var filterBuilder = Builders<FeatureFlag>.Filter;

            var filters = new List<FilterDefinition<FeatureFlag>>
            {
                // envId filter
                filterBuilder.Eq(flag => flag.EnvironmentId, envId)
            };

            // name filter
            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameFilter = filterBuilder.Where(flag => flag.FF.Name.ToLower().Contains(name.ToLower()));
                filters.Add(nameFilter);
            }

            // archive filter
            if (!includeArchived)
            {
                var unArchiveFilter = filterBuilder.Eq(flag => flag.IsArchived, false);
                filters.Add(unArchiveFilter);
            }
            
            // status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusFilter = filterBuilder.Where(flag => flag.FF.Status == status);
                filters.Add(statusFilter);
            }

            // flagIds filter
            if (flagIds != null)
            {
                var flagIdsFilter = filterBuilder.In(flag => flag.Id, flagIds.Distinct());
                filters.Add(flagIdsFilter);
            }

            var filter = filterBuilder.And(filters);

            var totalCount = await _mongoDb.CollectionOf<FeatureFlag>().CountDocumentsAsync(filter);

            var itemsQuery = _mongoDb.CollectionOf<FeatureFlag>()
                .Find(filter)
                .Project(flag => flag.FF)
                .SortByDescending(flag => flag.FF.LastUpdatedTime)
                .Skip(page * pageSize)
                .Limit(pageSize);

            var items = await itemsQuery.ToListAsync();

            return new PagedResult<FeatureFlagBasicInfo>(totalCount, items);
        }

        public async Task CreateDefaultAsync(
            int accountId,
            int projectId,
            int envId,
            string creatorId)
        {
            // set customized user properties
            var userProperty = new EnvironmentUserProperty
            {
                EnvironmentId = envId,
                Properties = new List<string> { "age" }
            };

            await _mongoDbServiceV1.CreateOrUpdateEnvironmentUserPropertiesForCRUDAsync(userProperty);

            var demoFeatureFlag = new CreateFeatureFlagViewModel
            {
                Name = "示例开关",
                Status = "Enabled",
                EnvironmentId = envId
            };

            await _mongoDbServiceV1.CreateDemoFeatureFlagAsync(demoFeatureFlag, creatorId, projectId, accountId);
        }
    }
}