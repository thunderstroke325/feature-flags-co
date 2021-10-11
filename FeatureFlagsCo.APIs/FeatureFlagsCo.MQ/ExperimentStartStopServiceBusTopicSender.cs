using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeatureFlagsCo.MQ
{
    public class ExperimentStartStopServiceBusTopicSender : ServiceBusTopicSenderBase
    {
        protected override string TopicPath { get { return "py.experiments.recordinginfo"; } }

        public ExperimentStartStopServiceBusTopicSender(IConfiguration configuration, ILogger<ExperimentStartStopServiceBusTopicSender> logger) : base(configuration, logger)
        {
        }
    }
}
