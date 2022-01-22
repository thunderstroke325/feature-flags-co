using System.Collections.Generic;

namespace FeatureFlags.APIs.ViewModels
{
    public class PagedResult<TValue>
    {
        public long TotalCount { get; set; }

        public IReadOnlyList<TValue> Items { get; set; }

        public PagedResult()
        {
        }

        public PagedResult(long totalCount, IReadOnlyList<TValue> items)
        {
            TotalCount = totalCount;
            Items = items ?? new List<TValue>();
        }
    }
}