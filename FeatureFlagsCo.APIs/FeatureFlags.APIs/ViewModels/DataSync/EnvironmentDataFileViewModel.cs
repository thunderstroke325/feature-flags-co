using Microsoft.AspNetCore.Http;

namespace FeatureFlags.APIs.ViewModels.DataSync
{
    public class EnvironmentDataFileViewModel
    {
        public UserUpdateModeEnum UserUpdateMode { get; set; } //Currently not used

        public IFormFile File { get; set; }
    }
}
