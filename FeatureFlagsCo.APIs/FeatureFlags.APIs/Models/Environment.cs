using System;
using System.ComponentModel.DataAnnotations;
using FeatureFlags.APIs.Services.MongoDb;

namespace FeatureFlags.APIs.Models
{
    public class Environment
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public string Description { get; set; }
        public string Secret { get; set; }
        public string MobileSecret { get; set; }
    }

    public class EnvironmentV2 : MongoDbIntIdEntity
    {
        public int ProjectId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Secret { get; set; }

        public string MobileSecret { get; set; }

        public EnvironmentV2(int projectId, string name, string description)
        {
            if (projectId == 0)
            {
                throw new ArgumentException("Environment projectId cannot be 0");
            }
            ProjectId = projectId;

            Update(name, description);
        }

        public void GenerateSecrets(int accountId)
        {
            if (Id == 0)
            {
                throw new InvalidOperationException(
                    "cannot generate secrets for environment because it's Id is empty." +
                    "normally you should save the environment to db before generating secrets.");
            }

            var envSecret = new EnvironmentSecretV2(accountId, Id, ProjectId);

            Secret = envSecret.New("default");
            MobileSecret = envSecret.New("mobile");
        }
        
        public string RegenerateSecret(string keyType, int accountId)
        {
            var envSecret = new EnvironmentSecretV2(accountId, Id, ProjectId);

            string newKey;
            switch (keyType)
            {
                case "Secret":
                    newKey = envSecret.New("default");
                    Secret = newKey;
                    break;
                case "MobileSecret":
                    newKey = envSecret.New("mobile");
                    MobileSecret = newKey;
                    break;
                default:
                    throw new ArgumentException("keyType must be either 'Secret' or 'MobileSecret'");
            }

            return newKey;
        }

        public void Update(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Environment name cannot be null or whitespace");
            }
            Name = name;

            Description = description;
        }
    }
}