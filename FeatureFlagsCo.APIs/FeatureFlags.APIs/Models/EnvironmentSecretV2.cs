using System;
using System.Globalization;
using System.Linq;

namespace FeatureFlags.APIs.Models
{
    public class EnvironmentSecretV2
    {
        public int AccountId { get; set; }

        public int EnvId { get; set; }

        public int ProjectId { get; set; }

        public EnvironmentSecretV2(
            int accountId,
            int envId,
            int projectId)
        {
            var ids = new[] { accountId, envId, projectId };
            if (ids.Any(id => id == 0))
            {
                throw new ArgumentException("accountId/envId/projectId cannot be 0");
            }

            AccountId = accountId;
            EnvId = envId;
            ProjectId = projectId;
        }

        public string New(string device)
        {
            var tradeTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);
            var guid = Guid.NewGuid().ToString();

            var originText =
                $"{guid.Substring(5, 10)}%{tradeTime}" +
                $"__{AccountId}" +
                $"__{ProjectId}" +
                $"__{EnvId}" +
                $"__{device}_{guid.Substring(0, 5)}";

            var textBytes = System.Text.Encoding.UTF8.GetBytes(originText);

            return Convert.ToBase64String(textBytes);
        }

        public static EnvironmentSecretV2 Parse(string envSecret)
        {
            var originTextBytes = Convert.FromBase64String(envSecret);
            var originText = System.Text.Encoding.UTF8.GetString(originTextBytes);

            var parts = originText.Split("__");
            var ids = new[] { parts[1], parts[2], parts[3] };
            if (parts.Length != 5 || ids.Any(id => !int.TryParse(id, out _)))
            {
                throw new ArgumentException("envSecret is not valid");
            }

            var accountId = int.Parse(ids[0]);
            var projectId = int.Parse(ids[1]);
            var envId = int.Parse(ids[2]);

            var secret = new EnvironmentSecretV2(accountId, envId, projectId);
            return secret;
        }
    }
}