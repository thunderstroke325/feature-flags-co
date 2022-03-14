using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Services
{
    public class SegmentService : ITransientDependency
    {
        private readonly MongoDbObjectIdRepository<Segment> _segments;

        public SegmentService(MongoDbObjectIdRepository<Segment> segments)
        {
            _segments = segments;
        }

        public async Task<PagedResult<Segment>> GetListAsync(
            int envId, 
            string name,
            int page,
            int pageSize)
        {
            var filterBuilder = Builders<Segment>.Filter;
            
            var filters = new List<FilterDefinition<Segment>>
            {
                // envId filter
                filterBuilder.Eq(segment => segment.EnvId, envId)
            };
            
            // name filter
            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameFilter = filterBuilder.Where(segment => segment.Name.ToLower().Contains(name.ToLower()));
                filters.Add(nameFilter);
            }
            
            var filter = filterBuilder.And(filters);
            
            var totalCount = await _segments.Collection.CountDocumentsAsync(filter);
            var itemsQuery = _segments.Collection
                .Find(filter)
                .SortByDescending(segment => segment.UpdatedAt)
                .Skip(page * pageSize)
                .Limit(pageSize);
            
            var items = await itemsQuery.ToListAsync();

            return new PagedResult<Segment>(totalCount, items);
        }

        public async Task<Segment> GetAsync(string id)
        {
            var objectId = ObjectId.Parse(id);

            var segment = await _segments.FirstOrDefaultAsync(x => x.Id == objectId);
            if (segment == null)
            {
                throw new EntityNotFoundException($"segment with id {id} not found");
            }

            return segment;
        }

        public async Task<Segment> CreateAsync(Segment segment)
        {
            var added = await _segments.AddAsync(segment);
            return added;
        }

        public async Task<Segment> UpdateAsync(Segment segment)
        {
            var updated = await _segments.UpdateAsync(segment);
            return updated;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var objectId = ObjectId.Parse(id);

            var isDeleted = await _segments.DeleteAsync(objectId);
            return isDeleted;
        }

        public async Task<bool> IsNameUsedAsync(int envId, string name)
        {
            var isNameUsed = await _segments.Queryable
                .AnyAsync(segment => segment.EnvId == envId && segment.Name == name);

            return isNameUsed;
        }
    }
}