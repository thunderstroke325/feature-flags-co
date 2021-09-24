using System.ComponentModel.DataAnnotations;
using System;

namespace FeatureFlags.APIs.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }
        public string OrganizationName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
