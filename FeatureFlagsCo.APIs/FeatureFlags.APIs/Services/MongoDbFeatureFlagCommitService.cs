using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbFeatureFlagCommitService: MongoCollectionServiceBase<FeatureFlagCommit>
    {
        public MongoDbFeatureFlagCommitService(IMongoDbSettings settings) : base(settings) { }


        public async Task<List<FeatureFlagCommit>> GetApprovalRequestsAsync(string featureFlagId, int pageIndex, int pageSize)
        {
            return await _collection.Find(p => p.FeatureFlagId == featureFlagId &&
                (p.ApprovalRequest.ReviewStatus == ReviewStatusEnum.Pending ||
                 p.ApprovalRequest.ReviewStatus == ReviewStatusEnum.Approved)).SortByDescending(p => p.CreatedAt).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
        }
    }
}
