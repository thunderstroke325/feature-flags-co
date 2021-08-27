using Microsoft.AspNetCore.Http;

namespace FeatureFlags.APIs.ViewModels.DataSync
{
    public class EnvironmentDataFileViewModel
    {
        public IFormFile File { get; set; }
    }
}
