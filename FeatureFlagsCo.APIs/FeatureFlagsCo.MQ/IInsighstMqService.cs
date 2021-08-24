using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FeatureFlagsCo.MQ
{
    public interface IInsighstMqService
    {
        void SendMessage(MessageModel message);
        Task SendMessageAsync(MessageModel message);
    }
}
