using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FeatureFlagsCo.MQ
{
    public interface IExperimentMqService
    {
        void SendMessage(ExperimentMessageModel message);
    }
}
