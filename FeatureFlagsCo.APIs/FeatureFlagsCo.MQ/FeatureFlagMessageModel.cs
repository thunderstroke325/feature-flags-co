namespace FeatureFlagsCo.MQ
{
    public class FeatureFlagMessageModel
    {
        public string RequestPath { get; set; }

        public string FeatureFlagId { get; set;}

        public string EnvId { get; set;}

        public string AccountId { get; set;}

        public string ProjectId { get; set;}

        public string FeatureFlagKeyName { get; set;}

        public string UserKeyId { get; set;}

        public string FFUserName { get; set;}

        public string VariationLocalId { get; set;}

        public string VariationValue { get; set;}
        
        public string TimeStamp { get; set; }
    }
}