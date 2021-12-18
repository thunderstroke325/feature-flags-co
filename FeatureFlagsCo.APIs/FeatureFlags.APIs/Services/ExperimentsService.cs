﻿using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.Experiments;
using FeatureFlags.APIs.ViewModels.Metrics;
using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public interface IExperimentsService
    {
        Task ArchiveExperiment(string experimentId);
        Task ArchiveExperimentData(string experimentId);
        Task<ExperimentViewModel> CreateExperiment(ExperimentViewModel param);
        Task<ExperimentIteration> StartIteration(int envId, string experimentId);
        Task<ExperimentIteration> StopIteration(int envId, string exptId, string iterationId);

        Task<string> GetEnvironmentEvents(int envId, MetricTypeEnum metricType, string lastItem = "",
            string searchText = "", int pageSize = 20);

        Task<List<ExperimentViewModel>> GetExperimentsByFeatureFlagIds(IEnumerable<string> featureFlagIds, bool shouldIncludeIterations);
        Task<IEnumerable<ExperimentIteration>> GetIterationResults(int envId, List<ExperimentIterationTuple> experimentIterationTuples);

        Task<IEnumerable<ExperimentMetricSetting>> GetActiveExperimentMetricSettingsAsync(int envId);
    }

    public class ExperimentsService : IExperimentsService
    {
        private readonly IOptions<MySettings> _mySettings;
        private readonly INoSqlService _noSqlDbService;
        private readonly MetricService _metricService;
        private readonly MessagingService _messagingService;
        private readonly MongoDbExperimentService _mongoDbExperimentService;
        private readonly MongoDbFeatureFlagService _mongoDbFeatureFlagService;

        public ExperimentsService(
            INoSqlService noSqlDbService,
            MessagingService messagingService,
            MetricService metricService,
            MongoDbExperimentService mongoDbExperimentService,
            MongoDbFeatureFlagService mongoDbFeatureFlagService,
            IOptions<MySettings> mySettings)
        {
            _noSqlDbService = noSqlDbService;
            _mySettings = mySettings;
            _metricService = metricService;
            _mongoDbExperimentService = mongoDbExperimentService;
            _mongoDbFeatureFlagService = mongoDbFeatureFlagService;
            _messagingService = messagingService;
        }

        public async Task<IEnumerable<ExperimentMetricSetting>> GetActiveExperimentMetricSettingsAsync(int envId)
        {
            var expts = await _mongoDbExperimentService.GetActiveExperimentsByEnvAsync(envId);
            var ActiveFeatureFlagIds = (await _mongoDbFeatureFlagService.GetActiveByIdsAsync(expts.Select(s => s.FlagId))).Select(ff => ff.Id);

            var exptIds = expts.Where(expt => ActiveFeatureFlagIds.Contains(expt.FlagId)).Select(expt => expt.MetricId);

            if (exptIds.Count() > 0)
            {
                return (await _metricService.GetMetricsByIdsAsync(exptIds))
                   .FindAll(m => m.EventType == EventType.Click || m.EventType == EventType.PageView)
                   .Select(m => new ExperimentMetricSetting
                   {
                       EventName = m.EventName,
                       EventType = m.EventType,
                       ElementTargets = m.ElementTargets,
                       TargetUrls = m.TargetUrls.Select(t => new TargetUrl { MatchType = t.MatchType, Url = t.Url }).ToList()
                   });
            }

            return new List<ExperimentMetricSetting>();
        }

        public async Task<List<ExperimentViewModel>> GetExperimentsByFeatureFlagIds(IEnumerable<string> featureFlagIds, bool shouldIncludeIterations)
        {
            var experiments = new List<Experiment>();

            foreach (var id in featureFlagIds) 
            {
                experiments.AddRange(await _noSqlDbService.GetExperimentByFeatureFlagAsync(id));
            }

            var metrics = (await _metricService.GetMetricsByIdsAsync(experiments.Select(ex => ex.MetricId)))
                .Select(m => new MetricViewModel 
                {
                    Id = m.Id,
                    Name = m.Name,
                    EventName = m.EventName,
                    EventType = m.EventType,
                    CustomEventTrackOption = m.CustomEventTrackOption,
                    CustomEventUnit = m.CustomEventUnit,
                    CustomEventSuccessCriteria = m.CustomEventSuccessCriteria
                });

            return experiments.OrderByDescending(ex => ex.CreatedAt).Select(r => {
                ExperimentStatus status = ExperimentStatus.NotStarted;
                var iterations = r.Iterations.Where(it => !it.IsArvhived).ToList();

                if (iterations.Count > 0) 
                {
                    var lastIteration = r.Iterations.Last();
                    if (lastIteration.EndTime.HasValue)
                    {
                        status = ExperimentStatus.NotRecording;
                    }
                    else 
                    {
                        status = ExperimentStatus.Recording;
                    }
                }

                var metric = metrics.FirstOrDefault(m => m.Id == r.MetricId);

                return new ExperimentViewModel
                {
                    Id = r.Id,
                    FeatureFlagId = r.FlagId,
                    MetricId = r.MetricId,
                    Metric = metric,
                    Status = status,
                    Iterations = shouldIncludeIterations ? iterations : null,
                    BaselineVariation = r.BaselineVariation
                };
            }).ToList(); 
        }

        public async Task ArchiveExperiment(string experimentId)
        {
            var experiment = await _noSqlDbService.GetExperimentByIdAsync(experimentId);
            if (experiment != null)
            {
                var metric = await _metricService.GetAsync(experiment.MetricId);
                // If the experiment has active iteration
                var operationTime = DateTime.UtcNow;
                experiment.Iterations.ForEach(async i =>
                {
                    if (!i.EndTime.HasValue)
                    {
                        var message = new ExperimentIterationMessageViewModel
                        {
                            ExptId = experiment.Id,
                            EnvId = experiment.EnvId.ToString(),
                            IterationId = i.Id,
                            StartExptTime = i.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                            EndExptTime = operationTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                            EventName = metric.EventName,
                            FlagId = experiment.Id,
                            BaselineVariation = experiment.BaselineVariation,
                            Variations = experiment.Variations
                        };

                        _messagingService.SendExperimentStartEndDataWithoutResponse(message);
                    }
                });


                await _noSqlDbService.ArchiveExperimentAsync(experimentId, operationTime);
            }
        }

        public async Task ArchiveExperimentData(string experimentId)
        {
            var experiment = await _noSqlDbService.GetExperimentByIdAsync(experimentId);
            if (experiment != null)
            {
                var metric = await _metricService.GetAsync(experiment.MetricId);
                // If the experiment has active iteration
                var operationTime = DateTime.UtcNow;
                experiment.Iterations.ForEach(async i =>
                {
                    if (!i.EndTime.HasValue)
                    {
                        var message = new ExperimentIterationMessageViewModel
                        {
                            ExptId = experiment.Id,
                            EnvId = experiment.EnvId.ToString(),
                            IterationId = i.Id,
                            StartExptTime = i.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                            EndExptTime = operationTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                            EventName = metric.EventName,
                            FlagId = experiment.Id,
                            BaselineVariation = experiment.BaselineVariation,
                            Variations = experiment.Variations
                        };

                        _messagingService.SendExperimentStartEndDataWithoutResponse(message);
                    }
                });


                await _noSqlDbService.ArchiveExperimentDataAsync(experimentId);
            }
        }

        public async Task<ExperimentViewModel> CreateExperiment(ExperimentViewModel param)
        {
            var experiment = await _noSqlDbService.GetExperimentByFeatureFlagAndMetricAsync(param.FeatureFlagId, param.MetricId);

            if (experiment == null)
            {
                // create experiment in db
                experiment = new Experiment
                {
                    EnvId = param.EnvId,
                    MetricId = param.MetricId,
                    Iterations = new List<ExperimentIteration>(),
                    FlagId = param.FeatureFlagId,
                    BaselineVariation = param.BaselineVariation,
                    Variations = param.Variations
                };

                experiment = await _noSqlDbService.CreateExperimentAsync(experiment);
            }

            param.Id = experiment.Id;
            param.Status = ExperimentStatus.NotStarted;
            
            return param;
        }


        public async Task<ExperimentIteration> StartIteration(int envId, string experimentId)
        {
            // create experiment in db
            var experiment = await _noSqlDbService.GetExperimentByIdAsync(experimentId);

            if (experiment != null)
            {
                var operationTime = DateTime.UtcNow;
                
                if (experiment.Iterations == null)
                {
                    experiment.Iterations = new List<ExperimentIteration>();
                }

                var metric = await _metricService.GetAsync(experiment.MetricId);

                var iteration = new ExperimentIteration
                {
                    Id = Guid.NewGuid().ToString(),
                    StartTime = operationTime,
                    // EndTime, Don't need set end time as this is a start experiment signal
                    Results = new List<IterationResult>(),
                    CustomEventSuccessCriteria = metric.CustomEventSuccessCriteria,
                    CustomEventTrackOption = metric.CustomEventTrackOption,
                    CustomEventUnit = metric.CustomEventUnit,
                    EventType = (int)metric.EventType,
                    EventName = metric.EventName,
                };
                
                // stop active iterations
                experiment.Iterations.ForEach(async i =>
                {
                    if (!i.EndTime.HasValue)
                    {
                        i.EndTime = operationTime;
                        var message = new ExperimentIterationMessageViewModel
                        {
                            ExptId = experiment.Id,
                            EnvId = envId.ToString(),
                            IterationId = i.Id,
                            EventType = (int)metric.EventType,
                            CustomEventUnit = metric.CustomEventUnit,
                            CustomEventTrackOption = (int)metric.CustomEventTrackOption,
                            CustomEventSuccessCriteria = (int)metric.CustomEventSuccessCriteria,
                            StartExptTime = i.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                            EndExptTime = operationTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                            EventName = metric.EventName,

                            FlagId = experiment.FlagId,
                            BaselineVariation = experiment.BaselineVariation,
                            Variations = experiment.Variations
                        };

                        _messagingService.SendExperimentStartEndDataWithoutResponse(message);
                    }
                });

                experiment.Iterations.Add(iteration);

                await _noSqlDbService.UpsertExperimentAsync(experiment);

                var message = new ExperimentIterationMessageViewModel
                {
                    ExptId = experiment.Id,
                    EnvId = envId.ToString(),
                    IterationId = iteration.Id,
                    EventType = (int)metric.EventType,
                    CustomEventUnit = metric.CustomEventUnit,
                    CustomEventTrackOption = (int)metric.CustomEventTrackOption,
                    CustomEventSuccessCriteria = (int)metric.CustomEventSuccessCriteria,
                    StartExptTime = iteration.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                    EventName = metric.EventName,
                    FlagId = experiment.FlagId,
                    BaselineVariation = experiment.BaselineVariation,
                    Variations = experiment.Variations
                };

                _messagingService.SendExperimentStartEndDataWithoutResponse(message);

                return iteration;
            }

            return null;
        }

        public async Task<IEnumerable<ExperimentIteration>> GetIterationResults(int envId, List<ExperimentIterationTuple> experimentIterationTuples)
        {
            var experiments = await _noSqlDbService.GetExperimentsByIdsAsync(experimentIterationTuples.Select(ex => ex.ExperimentId).ToList());

            if (experiments != null)
            {
                var iterationIds = experimentIterationTuples.Select(ex => ex.IterationId).ToList();
                return 
                    experiments.SelectMany(ex => ex.Iterations)
                    .Where(it => !it.IsArvhived && iterationIds.Contains(it.Id))
                    .Select(it =>
                        new ExperimentIteration 
                        {
                            Id = it.Id,
                            StartTime = it.StartTime,
                            EndTime = it.EndTime,
                            UpdatedAt = it.UpdatedAt,
                            Results = it.Results,
                            CustomEventUnit = it.CustomEventUnit,
                            CustomEventTrackOption = it.CustomEventTrackOption
                        }
                   );
            }

            return null;
        }

        public async Task<ExperimentIteration> StopIteration(int envId, string experimentId, string iterationId)
        {
            // create experiment in db
            var experiment = await _noSqlDbService.GetExperimentByIdAsync(experimentId);

            if (experiment != null)
            {
                var iteration = experiment.Iterations.Find(i => i.Id == iterationId);
                iteration.EndTime = DateTime.UtcNow;

                await _noSqlDbService.UpsertExperimentAsync(experiment);
                var metric = await _metricService.GetAsync(experiment.MetricId);

                var message = new ExperimentIterationMessageViewModel
                {
                    ExptId = experiment.Id,
                    EnvId = envId.ToString(),
                    IterationId = iteration.Id,
                    EventType = (int)metric.EventType,
                    CustomEventUnit = metric.CustomEventUnit,
                    CustomEventTrackOption = (int)metric.CustomEventTrackOption,
                    CustomEventSuccessCriteria = (int)metric.CustomEventSuccessCriteria,
                    StartExptTime = iteration.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                    EndExptTime = iteration.EndTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                    EventName = metric.EventName,
                    FlagId = experiment.FlagId,
                    BaselineVariation = experiment.BaselineVariation,
                    Variations = experiment.Variations
                };

                _messagingService.SendExperimentStartEndDataWithoutResponse(message);

                return iteration;
            }

            return null;
        }

        public async Task<string> GetEnvironmentEvents(int envId, MetricTypeEnum metricType, string lastItem = "", string searchText = "", int pageSize = 20)
        {
            string esHost = _mySettings.Value.ElasticSearchHost;
            string indexTarget = "experiments";

            dynamic envIdMatch = new ExpandoObject();
            (envIdMatch as IDictionary<string, object>)["EnvironmentId.keyword"] = $"{envId}";

            dynamic boolClause = new ExpandoObject();
            boolClause.must = new List<dynamic>()
            {
                new {
                    match = new {
                        Type = "CustomEvent"
                    }
                },
                new {
                    match = envIdMatch
                },
            };

            if (!string.IsNullOrWhiteSpace(searchText)) 
            {
                boolClause.must.Add(new
                {
                    query_string = new
                    {
                        query = $"*{searchText}*",
                        default_field = "EventName"
                    }
                });
            }

            var compositeEO = new ExpandoObject();
            (compositeEO as IDictionary<string, object>)["sources"] = new List<dynamic>() {
                            new {
                                EventName = new {
                                    terms = new {
                                        field = "EventName.keyword"
                                    }
                                }
                            }
                        };


            (compositeEO as IDictionary<string, object>)["size"] = pageSize;

            if (!string.IsNullOrWhiteSpace(lastItem)) 
            {
                (compositeEO as IDictionary<string, object>)["after"] = new { 
                    EventName = lastItem
                };
            }

            var aggsEO = new
            {
                keys = new { 
                    composite = compositeEO
                }
            };

            dynamic queryEO = new ExpandoObject();
            (queryEO as IDictionary<string, object>)["bool"] = boolClause;

            dynamic sortEO = new ExpandoObject();
            (sortEO as IDictionary<string, object>)["EventName.keyword"] = new
            {
                order = "asc"
            };

            var body = new
            {
                size = 0,
                query = queryEO,
                sort = new List<dynamic> { sortEO },
                aggs = aggsEO
            };

            // to see how to create a query to fetch unique values, ref https://www.getargon.io/docs/articles/elasticsearch/unique-values.html

            using (var client = new HttpClient())
            {
                HttpContent content = new StringContent(JsonConvert.SerializeObject(body));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                if (esHost.Contains("@")) // esHost contains username and password 
                {
                    var startIndex = esHost.LastIndexOf("/") + 1;
                    var endIndex = esHost.LastIndexOf("@");
                    var credential = esHost.Substring(startIndex, endIndex - startIndex).Split(":");
                    var userName = credential[0];
                    var password = credential[1];

                    esHost = esHost.Substring(0, startIndex) + esHost.Substring(endIndex + 1);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                                                "Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{userName}:{password}")));
                }

                //由HttpClient发出异步Post请求
                HttpResponseMessage res = await client.PostAsync($"{esHost}/{indexTarget}/_search", content);
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await res.Content.ReadAsStringAsync();
                }
                return null;
            }
        }
    }
}
