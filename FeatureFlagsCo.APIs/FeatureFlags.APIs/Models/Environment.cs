using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.Utils.Helpers;

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

        public List<EnvironmentSettingV2> Settings { get; set; }

        public EnvironmentV2(int projectId, string name, string description)
        {
            if (projectId == 0)
            {
                throw new ArgumentException("Environment projectId cannot be 0");
            }
            ProjectId = projectId;

            Settings = new List<EnvironmentSettingV2>();

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

        public void UpsertSetting(EnvironmentSettingV2 newSetting)
        {
            var existingSetting = Settings.FirstOrDefault(x => x.Id == newSetting.Id);
            if (existingSetting != null)
            {
                existingSetting.Update(newSetting);
            }
            else
            {
                Settings.Add(newSetting);
            }
        }

        public void DeleteSetting(string id)
        {
            Settings.RemoveAll(x => x.Id == id);
        }
    }

    public class EnvironmentSettingV2
    {
        public string Id { get; set; }
        
        public string Type { get; set; }
        
        public string Key { get; set; }

        public string Value { get; set; }

        public string Tag { get; set; }

        public string Remark { get; set; }

        public EnvironmentSettingV2(
            string id, 
            string type, 
            string key, 
            string value, 
            string tag = null,
            string remark = null)
        {
            Check.NotNullOrWhiteSpace(id, nameof(id));
            Id = id;
            
            Update(type, key, value, tag, remark);
        }

        public void Update(
            string type, 
            string key, 
            string value, 
            string tag = null,
            string remark = null)
        {
            Check.NotNullOrWhiteSpace(type, nameof(type));
            Check.NotNullOrWhiteSpace(key, nameof(key));
            Check.NotNullOrWhiteSpace(value, nameof(value));
            
            Type = type;
            Key = key;
            Value = value;

            Tag = tag ?? string.Empty;
            Remark = remark ?? string.Empty;
        }

        public void Update(EnvironmentSettingV2 newSetting)
        {
            if (Id != newSetting.Id)
            {
                return;
            }
            
            Type = newSetting.Type;
            Key = newSetting.Key;
            Value = newSetting.Value;
            Tag = newSetting.Tag;
            Remark = newSetting.Remark;
        }

        public void WriteRemark(string remark)
        {
            Remark = remark ?? string.Empty;
        }
        
        public override string ToString()
        {
            return $"Type = {Type}, Key = {Key}, Value = {Value}, Tag = {Tag}, Remark = {Remark}";
        }
    }

    public class EnvironmentSettingTypes
    {
        public const string SyncUrls = "sync-urls";
    }
}