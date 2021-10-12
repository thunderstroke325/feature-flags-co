using System;
using System.Collections.Generic;

namespace FeatureFlagsCo.MQ
{
    public class APIServiceToMQServiceModel
    {
        public MessageModel Message { get; set; }
        public FeatureFlagMessageModel FFMessage { get; set; }
    }
}
