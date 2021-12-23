using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbAnalyticBoardService : MongoCollectionServiceBase<AnalyticBoard>
    {
        public MongoDbAnalyticBoardService(IMongoDbSettings settings) : base(settings) 
        {
            // Create index on EnvSecret
            var indexKeysDefinition = Builders<AnalyticBoard>.IndexKeys.Descending(m => m.EnvId);
            Task.Run(() => _collection.Indexes.CreateOneAsync(new CreateIndexModel<AnalyticBoard>(indexKeysDefinition))).Wait();
        }

        public async Task<AnalyticBoard> GetByEnvIdAsync(int envId)
        {
            return await _collection.Find(e => e.EnvId == envId).FirstOrDefaultAsync();
        }

        public async Task RemoveDataSourceAsync(string boardId, string dataSourceId)
        {
            var board = await GetAsync(boardId);
            if (board != null)
            {
                board.RemoveDataSource(dataSourceId);
                await UpdateAsync(board.Id, board);
            }
        }

        public async Task RemoveDataGroupAsync(string boardId, string groupId)
        {
            var board = await GetAsync(boardId);
            if (board != null)
            {
                board.RemoveDataGroup(groupId);
                await UpdateAsync(board.Id, board);
            }
        }
        
        public async Task RemoveAnalyticDimensionAsync(string boardId, string dimensionId)
        {
            var board = await GetAsync(boardId);
            if (board != null)
            {
                board.RemoveDimension(dimensionId);
                await UpdateAsync(board.Id, board);
            }
        }
    }
}
