namespace FeatureFlagsCo.MQ.ElasticSearch.DataModels
{
    public class DataDimension
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public override string ToString() => $"{Key}@{Value}";
    }
}