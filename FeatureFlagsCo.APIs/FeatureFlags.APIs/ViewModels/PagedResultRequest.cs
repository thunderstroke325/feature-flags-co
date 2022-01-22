namespace FeatureFlags.APIs.ViewModels
{
    public class PagedResultRequest
    {
        public int Page { get; set; } = 0;

        public int PageSize { get; set; } = 10;
    }
}