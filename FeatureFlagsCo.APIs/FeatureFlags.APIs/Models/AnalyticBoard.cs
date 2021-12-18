using System;
using System.Collections.Generic;
using System.Linq;
using FeatureFlagsCo.MQ.ElasticSearch.DataModels;
using Nest;
using static Nest.Infer;

namespace FeatureFlags.APIs.Models
{
    public class AnalyticBoard : MongoModelBase
    {
        public int EnvId { get; set; }
        public List<DataSourceDef> DataSourceDefs { get; set; }
        public List<DataGroup> DataGroups { get; set; }
        public List<AnalyticDimension> Dimensions { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public override string GetCollectionName()
        {
            return "AnalyticBoard";
        }

        public void UpsertDataSource(
            string dataSourceId, 
            string name, 
            string keyName,
            string dataType)
        {
            var oldDataSource = DataSourceDefs.FirstOrDefault(x => x.Id == dataSourceId);
            if (oldDataSource != null)
            {
                oldDataSource.Update(name, dataType);
            }
            else
            {
                var newDataSource = new DataSourceDef(dataSourceId, name, keyName, dataType);
                DataSourceDefs.Add(newDataSource);
            }
        }

        public void UpsertDataGroup(
            string dataGroupId,
            string name,
            DateTime? startTime,
            DateTime? endTime,
            List<DataItem> items)
        {
            var oldDataGroup = DataGroups.FirstOrDefault(x => x.Id == dataGroupId);
            if (oldDataGroup != null)
            {
                oldDataGroup.Update(name, startTime, endTime, items);
            }
            else
            {
                var newDataGroup = new DataGroup(dataGroupId, name, startTime, endTime, items);
                DataGroups.Add(newDataGroup);
            }
        }

        public void UpsertAnalyticDimension(string dimensionId, string key, string value)
        {
            var oldDataDimension = Dimensions.FirstOrDefault(x => x.Id == dimensionId);
            if (oldDataDimension != null)
            {
                oldDataDimension.Update(key, value);
            }
            else
            {
                var newDataDimension = new AnalyticDimension(dimensionId, key, value);
                Dimensions.Add(newDataDimension);
            }
        }

        public void RemoveDataGroup(string groupId)
        {
            DataGroups.RemoveAll(x => x.Id == groupId);
        }

        public void RemoveDataSource(string sourceId)
        {
            DataSourceDefs.RemoveAll(x => x.Id == sourceId);
        }

        public void RemoveDataDimension(string dimensionId)
        {
            Dimensions.RemoveAll(x => x.Id == dimensionId);
        }
    }

    public class DataGroup 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<DataItem> Items { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public DataGroup(
            string id,
            string name, 
            DateTime? startTime, 
            DateTime? endTime, 
            List<DataItem> items)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("data group id cannot be null or whitespace.");
            }
            Id = id;
            CreatedAt = DateTime.UtcNow;
            
            Update(name, startTime, endTime, items);
        }

        public void Update(
            string name, 
            DateTime? startTime, 
            DateTime? endTime, 
            List<DataItem> items)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("data group name cannot be null or whitespace.");
            }
            Name = name;
            
            StartTime = startTime;
            EndTime = endTime;
            Items = items ?? new List<DataItem>();
            
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public class DataItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DataSourceDef DataSource { get; set; }
        public string Unit { get; set; }
        public string Color { get; set; }
        public CalculationType CalculationType { get; set; }
        
        public string AggregationName => $"{DataSource.KeyName}_{CalculationType}".ToLower();

        public double? AggregationValue(AggregateDictionary aggregations)
        {
            var aggregate = aggregations.TryGetValue(AggregationName, out var agg) 
                ? agg as ValueAggregate 
                : null;

            return aggregate?.Value;
        }

        public AggregationContainer AggregationContainer()
        {
            AggregationContainer aggregation = CalculationType switch
            {
                CalculationType.Count => new ValueCountAggregation(
                    AggregationName, Field<Analytics>(analytics => analytics.Key)),

                CalculationType.Sum => new SumAggregation(
                    AggregationName, Field<Analytics>(analytics => analytics.Value)),

                CalculationType.Average => new AverageAggregation(
                    AggregationName, Field<Analytics>(analytics => analytics.Value)),

                _ => throw new ArgumentOutOfRangeException()
            };

            return aggregation;
        }
    }

    public enum CalculationType 
    {
        Undefined = 0,
        Count = 1,
        Sum = 2,
        Average = 3
    }

    public class DataSourceDef 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string DataType { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        protected DataSourceDef()
        {
        }

        public DataSourceDef(
            string id,
            string name,
            string keyName,
            string dataType)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("data source definition id cannot be null or whitespace.");
            }
            Id = id;

            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException("data source definition key name cannot be null or whitespace.");
            }
            KeyName = keyName;

            CreatedAt = DateTime.UtcNow;
            Update(name, dataType);
        }
        
        public void Update(string name, string dataType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("data source definition name cannot be null or whitespace.");
            }
            Name = name;
            
            DataType = dataType;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public class AnalyticDimension
    {
        public string Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public AnalyticDimension(string id, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("data dimension id cannot be null or whitespace.");
            }
            Id = id;
            CreateAt = DateTime.UtcNow;
            
            Update(key, value);
        }

        public void Update(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("data dimension key cannot be null or whitespace.");
            }
            Key = key;

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("data dimension value cannot be null or whitespace.");
            }
            Value = value;
            
            UpdateAt = DateTime.UtcNow;
        }
    }
}
