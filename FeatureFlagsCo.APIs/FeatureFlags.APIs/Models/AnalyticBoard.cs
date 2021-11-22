using System;
using System.Collections.Generic;

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
    }

    public class DataGroup 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<DataItem> Items { get; set; }
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
    }
}
