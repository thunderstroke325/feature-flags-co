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
            try
            {
                var originTextBytes = Convert.FromBase64String(envSecret);
                var originText = System.Text.Encoding.UTF8.GetString(originTextBytes);

                var parts = originText.Split("__");
                var accountId = int.Parse(parts[1]);
                var projectId = int.Parse(parts[2]);
                var envId = int.Parse(parts[3]);

                var secret = new EnvironmentSecretV2(accountId, envId, projectId);
                return secret;
            }
            catch (Exception ex)
            {
                throw new InvalidEnvSecretException($"envSecret '{envSecret}' is invalid, please check again.", ex);
            }
        }

        public static bool TryParse(string envSecret, out EnvironmentSecretV2 secret)
        {
            try
            {
                secret = Parse(envSecret);
                return true;
            }
            catch (Exception)
            {
                secret = null;
                return false;
            }
        }
    }

    public class InvalidEnvSecretException : Exception
    {
        public InvalidEnvSecretException(string message)
            : base(message)
        {
        }

        public InvalidEnvSecretException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}