namespace FeatureFlagsCo.MQ
{
    public class APIServiceToMQServiceModel
    {
        public bool SendToExperiment { get; set; }
        public MessageModel Message { get; set; }
        public FeatureFlagMessageModel FFMessage { get; set; }
    }
}
