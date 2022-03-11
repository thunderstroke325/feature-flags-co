using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
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

        public async Task<IEnumerable<Segment>> GetListAsync(int envId)
        {
            var segments = await _segments.Queryable
                .Where(x => x.EnvId == envId)
                .ToListAsync();
            
            return segments;
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
    }
}