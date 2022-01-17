using System;
using System.ComponentModel.DataAnnotations;
using FeatureFlags.APIs.Services.MongoDb;

namespace FeatureFlags.APIs.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
    }

    public class ProjectV2 : MongoDbIntIdEntity
    {
        public int AccountId { get; set; }
        
        public string Name { get; set; }

        public ProjectV2(int accountId, string name)
        {
            if (accountId == 0)
            {
                throw new ArgumentException("Project accountId cannot be 0");
            }
            AccountId = accountId;
            
            Update(name);
        }

        public void Update(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Project name cannot be empty");
            }
            Name = name;
        }
    }
}
