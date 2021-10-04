namespace FeatureFlagsCo.MQ
{
    public interface IFeatureFlagMqService
    {
        void SendMessage(FeatureFlagMessageModel message);
    }
}