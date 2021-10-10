using System.Threading.Tasks;

namespace FeatureFlagsCo.MQ
{
    public interface IFeatureFlagMqService
    {
        void SendMessage(FeatureFlagMessageModel message);
        Task SendMessageAsync(FeatureFlagMessageModel message);
    }
}