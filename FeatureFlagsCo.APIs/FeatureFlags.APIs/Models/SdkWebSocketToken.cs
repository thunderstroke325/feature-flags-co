using System;
using System.Collections.Generic;
using System.Linq;

namespace FeatureFlags.APIs.Models
{
    public class SdkWebSocketToken
    {
        public long Timestamp { get; set; }

        public string EnvSecret { get; set; }

        public static bool TryCreate(string tokenStr, out SdkWebSocketToken token)
        {
            token = null;

            try
            {
                token = Create(tokenStr);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static SdkWebSocketToken Create(string tokenStr)
        {
            var magicMap = new Dictionary<char, int>
            {
                { 'Q', 0 },
                { 'B', 1 },
                { 'W', 2 },
                { 'S', 3 },
                { 'P', 4 },
                { 'H', 5 },
                { 'D', 6 },
                { 'X', 7 },
                { 'Z', 8 },
                { 'U', 9 },
            };

            var header = tokenStr[..5];
            var payload = tokenStr[5..];

            var encodedPosition = header[..3];
            var positionStr = encodedPosition
                .Aggregate(string.Empty, (current, character) => current + magicMap[character])
                .TrimStart('0');

            var encodedLength = header[3..];
            var lengthStr = encodedLength
                .Aggregate(string.Empty, (current, character) => current + magicMap[character])
                .TrimStart('0');

            var position = int.Parse(positionStr.TrimStart('0'));
            var length = int.Parse(lengthStr.TrimStart('0'));

            var encodedTimestamp = payload.Substring(position, length);
            var timestampStr = encodedTimestamp
                .Aggregate(string.Empty, (current, character) => current + magicMap[character])
                .TrimStart('0');
            var timestamp = long.Parse(timestampStr);

            var envSecret = payload.Remove(position, length);
            if (envSecret.Length % 4 != 0)
            {
                envSecret += new string('=', 4 - envSecret.Length % 4);
            }

            // make sure the envSecret is valid
            EnvironmentSecretV2.Parse(envSecret);

            return new SdkWebSocketToken
            {
                Timestamp = timestamp,
                EnvSecret = envSecret
            };
        }
    }
}