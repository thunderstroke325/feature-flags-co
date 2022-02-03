namespace FeatureFlags.APIs.ViewModels
{
    public class PagedResultRequest
    {
        public int PageIndex { get; set; } = 0;

        public int PageSize { get; set; } = 10;
    }
}