using System.ComponentModel.DataAnnotations;
using System;
using FeatureFlags.APIs.Services.MongoDb;

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

    public class AccountV2 : MongoDbIntIdEntity
    {
        public string OrganizationName { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public AccountV2(string organizationName)
        {
            CreatedAt = DateTime.UtcNow;

            Update(organizationName);
        }

        public void Update(string organizationName)
        {
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new ArgumentException("organization name cannot be null or empty", nameof(organizationName));
            }
            OrganizationName = organizationName;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}