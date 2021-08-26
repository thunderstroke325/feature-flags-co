using System;
using System.Collections.Generic;

namespace FeatureFlagsCo.MQ
{
    public class MessageModel
    {
        public List<MessageLabel> Labels { get; set; }
        public string Message { get; set; }
        public string FeatureFlagId { get; set; }
        public DateTime SendDateTime { get; set; }
        public string IndexTarget { get; set; }
    }

    public class MessageLabel
    {
        public string LabelName { get; set; }
        public string LabelValue { get; set; }
    }
}
