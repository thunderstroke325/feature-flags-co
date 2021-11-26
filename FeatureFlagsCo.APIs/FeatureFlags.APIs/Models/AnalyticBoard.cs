using System;
using System.Collections.Generic;
using System.Linq;

namespace FeatureFlags.APIs.Models
{
    public class AnalyticBoard : MongoModelBase
    {
        public int EnvId { get; set; }
        public List<DataSourceDef> DataSourceDefs { get; set; }
        public List<DataGroup> DataGroups { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public override string GetCollectionName()
        {
            return "AnalyticBoard";
        }

        public void UpsertDataSource(string dataSourceId, string name, string dataType)
        {
            var oldDataSource = DataSourceDefs.FirstOrDefault(x => x.Id == dataSourceId);
            if (oldDataSource != null)
            {
                oldDataSource.Update(name, dataType);
            }
            else
            {
                var newDataSource = new DataSourceDef(name, dataType);
                DataSourceDefs.Add(newDataSource);
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
    }

    public class DataItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DataSourceDef DataSource { get; set; }
        public string Unit { get; set; }
        public string Color { get; set; }
        public CalculationType CalculationType { get; set; }
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
        public string DataType { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }

        protected DataSourceDef()
        {
        }
        
        public DataSourceDef(string name, string dataType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("data source definition name cannot be null or whitespace.");
            }
            Name = name;
            
            DataType = dataType;
            
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
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
}
