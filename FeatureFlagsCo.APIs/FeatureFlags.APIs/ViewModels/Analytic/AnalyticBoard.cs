using System;
using System.Collections.Generic;

namespace FeatureFlags.APIs.Models
{
    public class AnalyticBoardViewModel
    {
        public string Id { get; set; }
        public int EnvId { get; set; }
        public List<DataSourceDef> DataSourceDefs { get; set; }
        public List<DataGroupViewModel> DataGroups { get; set; }
    }

    public class DataSourceDefViewModel 
    {
        public string AnalyticBoardId { get; set; }
        public int EnvId { get; set; }
        public List<DataSourceDef> DataSourceDefs { get; set; }
    }

    public class DataGroupViewModel
    {
        public string AnalyticBoardId {get;set;}
        public int EnvId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<DataItem> Items { get; set; }
    }

    public class CalculationParam
    {
        public int EnvId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public CalculationType CalculationType { get; set; }
        public List<DataSourceDef> Items { get; set; }
    }

    public class CalculationItemResultViewModel 
    {
        public string Id { get; set; }
        public double Value { get; set; }
    }

    public class CalculationResultsViewModel
    {
        public IEnumerable<CalculationItemResultViewModel> Items { get; set; }
    }
}
