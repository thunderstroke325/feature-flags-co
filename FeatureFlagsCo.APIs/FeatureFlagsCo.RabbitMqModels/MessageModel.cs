using System;
using System.Collections.Generic;

namespace FeatureFlagsCo.RabbitMqModels
{
    public class MessageModel
    {
        public List<MessageLabel> Labels { get; set; }
        public string Message { get; set; }
        public DateTime SendDateTime { get; set; }
    }

    public class MessageLabel
    {
        public string LabelName { get; set; }
        public string LabelValue { get; set; }
    }
}
