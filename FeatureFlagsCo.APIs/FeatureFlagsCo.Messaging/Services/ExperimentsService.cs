using FeatureFlagsCo.Messaging.Models;
using FeatureFlagsCo.Messaging.Services;
using FeatureFlagsCo.Messaging.ViewModels;
using FeatureFlagsCo.MQ;
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
            Task.Run(() => _collection.Indexes.CreateOneAsync(new CreateIndexModel<Experiment>(indexKeysDefinition)))
                .Wait();
        }

        public async Task<bool> UpdateExperimentResultAsync(ExperimentResult param)
        {
            var experiment = await GetAsync(param.ExperimentId);
            if (experiment != null)
            {
                var iteration = experiment.Iterations.Find(it => it.Id == param.IterationId);
                if (iteration != null)
                {
                    if (param.EventType.HasValue)
                    {
                        iteration.CustomEventSuccessCriteria = param.CustomEventSuccessCriteria.Value;
                        iteration.CustomEventTrackOption = param.CustomEventTrackOption.Value;
                        iteration.EventType = (int) param.EventType.Value;
                    }
                    else // 历史遗留实验中，EventType等为 null
                    {
                        iteration.CustomEventSuccessCriteria = CustomEventSuccessCriteria.Higher;
                        iteration.CustomEventTrackOption = CustomEventTrackOption.Conversion;
                        iteration.EventType = (int) EventType.Custom;
                    }

                    iteration.CustomEventUnit = param.CustomEventUnit;
                    iteration.UpdatedAt = param.EndTime;
                    iteration.Results = param.Results;

                    await UpsertItemAsync(experiment);
                    return true;
                }
            }
            return false;
        }

        public void UpdateExperimentResult(ExperimentResult param)
        {
            var experiment = Get(param.ExperimentId);
            if (experiment != null)
            {
                var iteration = experiment.Iterations.Find(it => it.Id == param.IterationId);

                if (iteration != null)
                {
                    if (param.EventType.HasValue)
                    {
                        iteration.CustomEventSuccessCriteria = param.CustomEventSuccessCriteria.Value;
                        iteration.CustomEventTrackOption = param.CustomEventTrackOption.Value;
                        iteration.EventType = (int) param.EventType.Value;
                    }
                    else // 历史遗留实验中，EventType等为 null
                    {
                        iteration.CustomEventSuccessCriteria = CustomEventSuccessCriteria.Higher;
                        iteration.CustomEventTrackOption = CustomEventTrackOption.Conversion;
                        iteration.EventType = (int) EventType.Custom;
                    }

                    iteration.CustomEventUnit = param.CustomEventUnit;
                    iteration.UpdatedAt = param.EndTime;
                    iteration.Results = param.Results;

                    UpsertItem(experiment);
                }
            }
        }
    }
}