using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FeatureFlagsCo.MQ
{
    public interface IExperimentMqService
    {
        bool SendMessage(ExperimentMessageModel message);
        Task<bool> SendMessageAsync(ExperimentMessageModel message);
    }
}
