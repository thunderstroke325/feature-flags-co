using System;

namespace FeatureFlags.APIs.ViewModels.DataSync
{
    [Flags]
    public enum DownloadOptionEnum
    {
        None = 0,
        FeatureFlags = 1,
        Users = 2,
        UserProperties = 4
    }
}
