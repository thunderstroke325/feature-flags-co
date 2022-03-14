using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.ViewModels
{
    public class UpsertSegment
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(128)]
        public string Name { get; set; }
        
        [StringLength(256)]
        public string Description { get; set; }

        public IEnumerable<string> Included { get; set; } = Array.Empty<string>();

        public IEnumerable<string> Excluded { get; set; } = Array.Empty<string>();
    }
    
    public class SegmentVm
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        
        public IEnumerable<string> Included { get; set; } = Array.Empty<string>();
        
        public IEnumerable<string> Excluded { get; set; } = Array.Empty<string>();
    }

    public class SegmentListItem
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class SearchSegmentRequest : PagedResultRequest
    {
        public string Name { get; set; }
    }
}