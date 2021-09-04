using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public static class FeatureFlagKeyExtension
    {

        public static string EncodeBase64(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string DecodeBase64(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string GetEnvironmentUserPropertyId(int envId)
        {
            return $"WUP__{envId}";
        }

        public static string GetAccountFeatureFlagUserId(int accountId, string ffUserKeyId)
        {
            return $"WFFU__{accountId.ToString()}__{ffUserKeyId}";
        }

        public static string GetFeatureFlagUserId(string featureFlagId, string ffUserKeyId )
        {
            return $"FFUM__{featureFlagId}__{ffUserKeyId}";
        }

        public static string GetEnvironmentUserId(int accountId, string userKeyId)
        {
            return $"WU__{accountId.ToString()}__{userKeyId}";
        }

        public static string GenerateEnvironmentKey(int envId, int accountId = -1, int projectId = -1, string device = "default")
        {
            string tradeTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);
            string guid = Guid.NewGuid().ToString();
            string keyOriginText = $"{guid.Substring(5, 10)}%{tradeTime}__{accountId}__{projectId}__{envId}__{device}_{guid.Substring(0, 5)}";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(keyOriginText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string CreateNewFeatureFlagKeyName(int environmentId, string featureFlagName)
        {
            return featureFlagName.Replace(" ", "-").Replace("/", "-").Replace("\\", "-").Replace(".", "-").Replace(":", "-").Replace("_", "").Replace("'", "").Replace("\"", "");
        }

        public static string GetFeatureFlagId(string featureFlagKeyName, string envId, string accountId = "-1", string projectId = "-1")
        {
            return $"FF__{accountId}__{projectId}__{envId}__{featureFlagKeyName}";
        }

        public static FeatureFlagIdByEnvironmentKeyViewModel GetFeatureFlagIdByEnvironmentKey(string envKey, string featureFlagKeyName)
        {
            var keyOriginTextByte = System.Convert.FromBase64String(envKey);
            var keyOriginText = System.Text.Encoding.UTF8.GetString(keyOriginTextByte).Split("__");
            var accountId = keyOriginText[1];
            var projectId = keyOriginText[2];
            var envId = keyOriginText[3];

            return new FeatureFlagIdByEnvironmentKeyViewModel
            {
                FeatureFlagId = GetFeatureFlagId(featureFlagKeyName, envId, accountId, projectId),
                EnvId = envId,
                AccountId = accountId,
                ProjectId = projectId
            };
        }
        
        public static int GetEnvIdByFeautreFlagId(string featureFlagId)
        {
            return Convert.ToInt32(featureFlagId.Split("__")[3]);
        }
        public static string GetFeautreFlagKeyById(string featureFlagId)
        {
            return featureFlagId.Split("__")[4].Trim();
        }
        public static FeatureFlagIdByEnvironmentKeyViewModel GetEnvIdsByEnvKey(string envKey)
        {
            var keyOriginTextByte = System.Convert.FromBase64String(envKey);
            var keyOriginText = System.Text.Encoding.UTF8.GetString(keyOriginTextByte).Split("__");
            var accountId = keyOriginText[1];
            var projectId = keyOriginText[2];
            var envId = keyOriginText[3];
            return new FeatureFlagIdByEnvironmentKeyViewModel
            {
                AccountId = accountId,
                EnvId = envId,
                FeatureFlagId = null,
                ProjectId = projectId
            };
        }
    }

    public class FeatureFlagIdByEnvironmentKeyViewModel
    {
        public string FeatureFlagId { get; set; }
        public string EnvId { get; set; }
        public string AccountId { get; set; }
        public string ProjectId { get; set; }
    }
}
