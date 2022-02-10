using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FeatureFlags.Utils.ExtensionMethods;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FeatureFlags.APIs.Models
{
    public class SdkWebSocketMessage
    {
        public string MessageType { get; protected set; }

        public object Data { get; protected set; }

        public SdkWebSocketMessage(string messageType, object data)
        {
            MessageType = messageType;
            Data = data;
        }

        public string AsJson()
        {
            var setting = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var json = JsonConvert.SerializeObject(this, setting);
            return json;
        }
    }

    public class SdkDataSync
    {
        [CanBeNull] 
        public FeatureFlagUser User { get; set; }

        public long Timestamp { get; set; }
    }

    public class ServerSdkFeatureFlag
    {
        [JsonPropertyName("_id")] public string _Id { get; set; }

        [JsonPropertyName("id")] public string Id { get; set; }

        public int EnvironmentId { get; set; }

        public bool IsArchived { get; set; }

        public FeatureFlagBasicInfo FF { get; set; }

        public List<FeatureFlagPrerequisite> FFP { get; set; }

        public List<FeatureFlagTargetUsersWhoMatchTheseRuleParam> FFTUWMTR { get; set; }

        public List<TargetIndividualForVariationOption> TargetIndividuals { get; set; }

        public List<VariationOption> VariationOptions { get; set; }

        public bool? ExptIncludeAllRules { get; set; }

        public long Timestamp => FF.LastUpdatedTime?.UnixTimestampInMilliseconds() ?? 0;
    }

    public class ClientSdkFeatureFlag
    {
        public string Id { get; set; }

        public string Variation { get; set; }

        public bool SendToExperiment { get; set; }

        public bool IsArchived { get; set; }

        public IEnumerable<ClientSdkVariation> Options { get; set; }

        public long Timestamp { get; set; }
    }

    public class ClientSdkVariation
    {
        public int Id { get; set; }

        public string Value { get; set; }
    }

    public class SdkWebSocketMessageHandleException : Exception
    {
        public SdkWebSocketMessageHandleException(string message) : base(message)
        {
        }

        public SdkWebSocketMessageHandleException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}