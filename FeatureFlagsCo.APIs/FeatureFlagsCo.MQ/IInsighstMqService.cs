using System;
using System.Collections.Generic;
using System.Text;

namespace FeatureFlagsCo.MQ
{
    public interface IInsighstMqService
    {
        void SendMessage(MessageModel message);
    }
}
