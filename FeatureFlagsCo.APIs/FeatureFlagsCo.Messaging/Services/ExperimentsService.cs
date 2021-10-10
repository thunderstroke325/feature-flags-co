
using FeatureFlagsCo.Messaging.Models;
using FeatureFlagsCo.Messaging.Services;
using FeatureFlagsCo.Messaging.ViewModels;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging.Services
{

    public class ExperimentsService : MongoCollectionServiceBase<Experiment>
    {
        public ExperimentsService(IMongoDbSettings settings) : base(settings)
        {
            // Create index on updatedAt
            var indexKeysDefinition = Builders<Experiment>.IndexKeys.Descending(m => m.CreatedAt);
            Task.Run(() => _collection.Indexes.CreateOneAsync(new CreateIndexModel<Experiment>(indexKeysDefinition))).Wait();
        }

        public async Task UpdateExperimentResultAsync(ExperimentResult param) 
        {
            var experiment = await GetAsync(param.ExperimentId);
            if (experiment != null) 
            {
                var iteration = experiment.Iterations.Find(it => it.Id == param.IterationId);
                iteration.UpdatedAt = param.EndTime;
                iteration.Results = param.Results;

                await UpsertItemAsync(experiment);
            }
        }

        public void UpdateExperimentResult(ExperimentResult param)
        {
            var experiment = Get(param.ExperimentId);
            if (experiment != null)
            {
                var iteration = experiment.Iterations.Find(it => it.Id == param.IterationId);
                iteration.UpdatedAt = param.EndTime;
                iteration.Results = param.Results;

                UpsertItem(experiment);
            }
        }
    }
}
