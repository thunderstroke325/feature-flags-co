using System;
using System.Collections.Generic;

namespace FeatureFlagsCo.MQ
{
    public class AuditLogMessageModel
    {
        public string Route { get; set; }
        public string MainMessage { get; set; }
        public List<string> ChangesMessages { get; set; }
        public long TimeStamp { get; set; }
        public AuditLogUserInfo User { get; set; }
        public List<MqCustomizedProperty> CustomizedProperties { get; set; }
        public string EnvironmentId { get; set; }
        public string ProjectId { get; set; }
        public string FeatureFlagId { get; set; }
        public string PostBody { get; set; }
    }

    public class AuditLogUserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> UserRoles { get; set; }
        public List<MqCustomizedProperty> CustomizedProperties { get; set; }
    }
}
