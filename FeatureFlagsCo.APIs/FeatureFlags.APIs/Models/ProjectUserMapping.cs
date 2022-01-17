using System;
using System.ComponentModel.DataAnnotations;
using FeatureFlags.APIs.Services.MongoDb;

namespace FeatureFlags.APIs.Models
{
    public class ProjectUserMapping
    {
        [Key]
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
    }

    public class ProjectUserV2 : MongoDbIntIdEntity
    {
        public int ProjectId { get; set; }
        
        public string UserId { get; set; }
        
        public string Role { get; set; }

        public ProjectUserV2(int projectId, string userId, string role)
        {
            if (projectId == 0)
            {
                throw new ArgumentException("ProjectUser projectId cannot be 0");
            }
            ProjectId = projectId;
            
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("ProjectUser userId cannot be null or empty");
            }
            UserId = userId;
            
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("ProjectUser role cannot be null or empty");
            }
            Role = role;
        }
    }
}
