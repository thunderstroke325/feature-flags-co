using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels
{
    public class MySettings
    {
        public string AdminWebPortalUrl { get; set; }
        public string SendCloudAPIUser { get; set; }
        public string SendCloudAPIKey { get; set; }
        public string SendCloudFrom { get; set; }
        public string SendCloudFromName { get; set; }
        public string SendCloudTemplate { get; set; }
        public string SendCloudEmailSubject { get; set; }
        public string TestSetting { get; set; }

        public string IDistributedCacheName { get; set; }

        public string AppInsightsApplicationId { get; set; }
        public string AppInsightsApplicationApiSecret { get; set; }

        public string InsightsRabbitMqUrl { get; set; }
        public string GrafanaLokiUrl { get; set; }
        public string HostingType { get; set; }
    }

    public enum HostingTypeEnum
    {
        Local,
        Azure
    }
}
