using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FeatureFlagsCo.MQ
{
    public interface IAuditLogMqService
    {
        void Log(AuditLogMessageModel message);
    }
}
