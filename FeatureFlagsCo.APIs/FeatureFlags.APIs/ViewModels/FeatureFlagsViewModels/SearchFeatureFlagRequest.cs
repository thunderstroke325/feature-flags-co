namespace FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels
{
    public class SearchFeatureFlagRequest : PagedResultRequest
    {
        public string Name { get; set; }

        public string Status { get; set; }

        public int[] TagIds { get; set; }

        public bool IncludeArchived { get; set; } = false;
    }
}