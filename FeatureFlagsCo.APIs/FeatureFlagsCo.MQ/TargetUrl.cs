using System;
using System.Collections.Generic;
using System.Text;

namespace FeatureFlagsCo.MQ
{
    public class TargetUrl
    {
        public string Id { get; set; }
        public UrlMatchType MatchType { get; set; }
        public string Url { get; set; }
    }
}
