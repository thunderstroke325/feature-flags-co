using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using FeatureFlags.APIs.ViewModels.Environment;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;

namespace FeatureFlags.APIs.Repositories
{
    public interface IVariationService
    {
        Task<Tuple<VariationOption, bool>> CheckMultiOptionVariationAsync(string environmentSecret, string featureFlagKeyName, EnvironmentUser ffUser,
            FeatureFlagIdByEnvironmentKeyViewModel ffIdVM);

        bool IfBelongRolloutPercentage(string userFFKeyId, double[] rolloutPercentageRange);
    }

    public class VariationService : IVariationService
    {
        private readonly IDistributedCache _redisCache;
        private readonly INoSqlService _nosqlDbService;

        public VariationService(
            IDistributedCache redisCache,
            INoSqlService nosqlDbService)
        {
            _redisCache = redisCache;
            _nosqlDbService = nosqlDbService;
        }

        #region Multi Options
        public async Task<Tuple<VariationOption, bool>> CheckMultiOptionVariationAsync(
            string environmentSecret, string featureFlagKeyName, EnvironmentUser environmentUser, FeatureFlagIdByEnvironmentKeyViewModel ffIdVM)
        {
            string featureFlagId = ffIdVM.FeatureFlagId;

            int environmentId = Convert.ToInt32(ffIdVM.EnvId);
            environmentUser.EnvironmentId = environmentId;
            environmentUser.Id = FeatureFlagKeyExtension.GetEnvironmentUserId(environmentId, environmentUser.KeyId);

            bool readOnlyOperation = true;

            // get feature flag info
            var featureFlagString = await _redisCache.GetStringAsync(featureFlagId);
            FeatureFlag featureFlag = null;
            if (!string.IsNullOrWhiteSpace(featureFlagString))
                featureFlag = JsonConvert.DeserializeObject<FeatureFlag>(featureFlagString);
            else
            {
                featureFlag = await _nosqlDbService.GetFeatureFlagAsync(featureFlagId);
                await _redisCache.SetStringAsync(featureFlagId, JsonConvert.SerializeObject(featureFlag));
                readOnlyOperation = false;
            }

            // get environment feature flag user info
            var featureFlagUserMappingId = FeatureFlagKeyExtension.GetFeatureFlagUserId(featureFlagId, environmentUser.KeyId);
            var featureFlagsUserMappingString = await _redisCache.GetStringAsync(featureFlagUserMappingId);
            EnvironmentFeatureFlagUser cosmosDBFeatureFlagsUser = null;

            if (!string.IsNullOrWhiteSpace(featureFlagsUserMappingString))
            {
                cosmosDBFeatureFlagsUser = JsonConvert.DeserializeObject<EnvironmentFeatureFlagUser>(featureFlagsUserMappingString);                

                if (featureFlag.FF.LastUpdatedTime != null && cosmosDBFeatureFlagsUser.LastUpdatedTime != null &&
                    featureFlag.FF.LastUpdatedTime.Value.CompareTo(cosmosDBFeatureFlagsUser.LastUpdatedTime.Value) <= 0)
                {
                    if (cosmosDBFeatureFlagsUser.UserInfo != null)
                    {
                        var userInfo = cosmosDBFeatureFlagsUser.UserInfo;
                        if ((environmentUser.CustomizedProperties == userInfo.CustomizedProperties ||
                            JsonConvert.SerializeObject(environmentUser.CustomizedProperties).Trim() == JsonConvert.SerializeObject(userInfo.CustomizedProperties).Trim()) &&
                            environmentUser.Name == userInfo.Name)
                        {
                            return new Tuple<VariationOption, bool>(cosmosDBFeatureFlagsUser.VariationOptionResultValue, readOnlyOperation);
                        }
                    }
                }
            }

            cosmosDBFeatureFlagsUser = await MultiOptionRedoMatchingAndUpdateToRedisCacheAsync(
                featureFlagId, featureFlagUserMappingId, featureFlag,
                environmentUser, environmentSecret, ffIdVM);

            cosmosDBFeatureFlagsUser.UserInfo = environmentUser;
            await _redisCache.SetStringAsync(featureFlagUserMappingId, JsonConvert.SerializeObject(cosmosDBFeatureFlagsUser));

            await UpsertEnvironmentUserAsync(environmentUser, environmentId);

            return new Tuple<VariationOption, bool>(cosmosDBFeatureFlagsUser.VariationOptionResultValue, false);
        }

        private async Task<EnvironmentFeatureFlagUser> MultiOptionRedoMatchingAndUpdateToRedisCacheAsync(
           string featureFlagId,
           string environmentFeatureFlagUserId,
           FeatureFlag featureFlag,
           EnvironmentUser environmentUser,
           string environmentSecret,
           FeatureFlagIdByEnvironmentKeyViewModel ffIdVM)
        {
            EnvironmentFeatureFlagUser environmentFeatureFlagUser = new EnvironmentFeatureFlagUser
                {
                    FeatureFlagId = featureFlagId,
                    EnvironmentId = environmentUser.EnvironmentId,
                    id = environmentFeatureFlagUserId
                };
            environmentFeatureFlagUser.LastUpdatedTime = DateTime.UtcNow;
            environmentFeatureFlagUser.VariationOptionResultValue = 
                await MultiOptionGetUserVariationResultAsync(
                        featureFlag, environmentUser, environmentFeatureFlagUser, environmentSecret, ffIdVM);
            return environmentFeatureFlagUser;
        }

        public async Task<VariationOption> MultiOptionGetUserVariationResultAsync(
            FeatureFlag cosmosDBFeatureFlag, 
            EnvironmentUser environmentUser,
            EnvironmentFeatureFlagUser environmentFeatureFlagUser,
            string environmentSecret,
            FeatureFlagIdByEnvironmentKeyViewModel ffIdVM)
        {
            var wsId = cosmosDBFeatureFlag.EnvironmentId;

            if (cosmosDBFeatureFlag.FF.Status == FeatureFlagStatutEnum.Disabled.ToString())
                return cosmosDBFeatureFlag.FF.VariationOptionWhenDisabled;

            // 判断Prequisite
            foreach (var ffPItem in cosmosDBFeatureFlag.FFP)
            {
                if (ffPItem.PrerequisiteFeatureFlagId != cosmosDBFeatureFlag.FF.Id)
                {
                    var r = await CheckMultiOptionVariationAsync(
                                    environmentSecret,
                                    FeatureFlagKeyExtension.GetFeautreFlagKeyById(ffPItem.PrerequisiteFeatureFlagId),
                                    environmentUser,
                                    new FeatureFlagIdByEnvironmentKeyViewModel()
                                    {
                                        AccountId = ffIdVM.AccountId,
                                        EnvId = ffIdVM.EnvId,
                                        FeatureFlagId = ffPItem.PrerequisiteFeatureFlagId,
                                        ProjectId = ffIdVM.ProjectId
                                    });
                    if (r.Item1.LocalId != ffPItem.ValueOptionsVariationValue.LocalId)
                        return cosmosDBFeatureFlag.FF.VariationOptionWhenDisabled;
                }
            }

            if(cosmosDBFeatureFlag.TargetIndividuals != null && cosmosDBFeatureFlag.TargetIndividuals.Count > 0)
            {
                foreach(var individualItem in cosmosDBFeatureFlag.TargetIndividuals)
                {
                    if (individualItem.Individuals.Any(p => p.KeyId == environmentUser.KeyId))
                        return individualItem.ValueOption;
                }
            }

            // 判断Match Rules
            var ffUserCustomizedProperties = environmentUser.CustomizedProperties ?? new List<FeatureFlagUserCustomizedProperty>();
            var ffTargetUsersWhoMatchRules = cosmosDBFeatureFlag.FFTUWMTR ?? new List<FeatureFlagTargetUsersWhoMatchTheseRuleParam>();
            var ruleMatchResult = MultiOptionGetUserVariationRuleMatchResult(
                cosmosDBFeatureFlag,
                environmentUser,
                environmentFeatureFlagUser);
            if (ruleMatchResult != null)
                return ruleMatchResult;

            // 判断Default Rule
            if (cosmosDBFeatureFlag.FF.DefaultRulePercentageRollouts != null)
            {
                foreach (var item in cosmosDBFeatureFlag.FF.DefaultRulePercentageRollouts)
                {
                    if (IfBelongRolloutPercentage(environmentUser.KeyId, item.RolloutPercentage))
                        return item.ValueOption;
                }
            }

            return cosmosDBFeatureFlag.FF.VariationOptionWhenDisabled;
        }

        private VariationOption MultiOptionGetUserVariationRuleMatchResult(
          FeatureFlag cosmosDBFeatureFlag,
          EnvironmentUser ffUser,
          EnvironmentFeatureFlagUser environmentFeatureFlagUser)
        {
            foreach (var ffTUWMRItem in cosmosDBFeatureFlag.FFTUWMTR)
            {

                var rules = ffTUWMRItem.RuleJsonContent;
                bool isInCondition = true;
                if (rules != null && rules.Count > 0)
                {
                    foreach (var rule in rules)
                    {
                        var customizedProperties = ffUser.CustomizedProperties;
                        if (ffUser.CustomizedProperties == null)
                            customizedProperties = new List<FeatureFlagUserCustomizedProperty>();
                        var ffUCProperty = customizedProperties.FirstOrDefault(p => p.Name == rule.Property);
                        if (ffUCProperty == null)
                            ffUCProperty = new FeatureFlagUserCustomizedProperty();
                        if (rule.Operation.Contains("Than") && ffUCProperty != null)
                        {
                            double conditionDoubleValue, ffUserDoubleValue;
                            if (!Double.TryParse(rule.Value, out conditionDoubleValue))
                            {
                                isInCondition = false;
                                break;
                            }
                            if (!Double.TryParse(ffUCProperty.Value, out ffUserDoubleValue))
                            {
                                isInCondition = false;
                                break;
                            }

                            if (rule.Operation == RuleTypeEnum.BiggerEqualThan.ToString() &&
                                Math.Round(ffUserDoubleValue, 5) >= Math.Round(conditionDoubleValue, 5))
                                continue;
                            else if (rule.Operation == RuleTypeEnum.BiggerThan.ToString() &&
                                Math.Round(ffUserDoubleValue, 5) > Math.Round(conditionDoubleValue, 5))
                                continue;
                            else if (rule.Operation == RuleTypeEnum.LessEqualThan.ToString() &&
                                Math.Round(ffUserDoubleValue, 5) <= Math.Round(conditionDoubleValue, 5))
                                continue;
                            else if (rule.Operation == RuleTypeEnum.LessThan.ToString() &&
                                Math.Round(ffUserDoubleValue, 5) < Math.Round(conditionDoubleValue, 5))
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.Equal.ToString())
                        {
                            if (rule.Property == "KeyId" &&
                                rule.Value == ffUser.KeyId)
                                continue;
                            else if (rule.Property == "Name" &&
                                     rule.Value == ffUser.Name)
                                continue;
                            else if (rule.Property == "Email" &&
                                     rule.Value == ffUser.Email)
                                continue;
                            else if (rule.Property == ffUCProperty.Name &&
                                     rule.Value == ffUCProperty.Value)
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.NotEqual.ToString())
                        {
                            if (rule.Property == "KeyId" &&
                                rule.Value != ffUser.KeyId)
                                continue;
                            else if (rule.Property == "Name" &&
                                     rule.Value != ffUser.Name)
                                continue;
                            else if (rule.Property == "Email" &&
                                     rule.Value != ffUser.Email)
                                continue;
                            else if (rule.Property == ffUCProperty.Name &&
                                     rule.Value != ffUCProperty.Value)
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.Contains.ToString())
                        {
                            if (rule.Property == "KeyId" &&
                                ffUser.KeyId.Contains(rule.Value))
                                continue;
                            else if (rule.Property == "Name" &&
                                ffUser.Name.Contains(rule.Value))
                                continue;
                            else if (rule.Property == "Email" &&
                                ffUser.Email.Contains(rule.Value))
                                continue;
                            else if (rule.Property == ffUCProperty.Name &&
                                ffUCProperty.Value.Contains(rule.Value))
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.NotContain.ToString())
                        {
                            if (rule.Property == "KeyId" &&
                                !ffUser.KeyId.Contains(rule.Value))
                                continue;
                            else if (rule.Property == "Name" &&
                                !ffUser.Name.Contains(rule.Value))
                                continue;
                            else if (rule.Property == "Email" &&
                                !ffUser.Email.Contains(rule.Value))
                                continue;
                            else if (rule.Property == ffUCProperty.Name &&
                                !ffUCProperty.Value.Contains(rule.Value))
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.IsOneOf.ToString())
                        {
                            var ruleValues = JsonConvert.DeserializeObject<List<string>>(rule.Value);
                            if (rule.Property == "KeyId" &&
                                ruleValues.Contains(ffUser.KeyId))
                                continue;
                            else if (rule.Property == "Name" &&
                                ruleValues.Contains(ffUser.Name))
                                continue;
                            else if (rule.Property == "Email" &&
                                ruleValues.Contains(ffUser.Email))
                                continue;
                            else if (rule.Property == ffUCProperty.Name &&
                                ruleValues.Contains(ffUCProperty.Value))
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.NotOneOf.ToString())
                        {
                            var ruleValues = JsonConvert.DeserializeObject<List<string>>(rule.Value);
                            if (rule.Property == "KeyId" &&
                                !ruleValues.Contains(ffUser.KeyId))
                                continue;
                            else if (rule.Property == "Name" &&
                                !ruleValues.Contains(ffUser.Name))
                                continue;
                            else if (rule.Property == "Email" &&
                                !ruleValues.Contains(ffUser.Email))
                                continue;
                            else if (rule.Property == ffUCProperty.Name &&
                                !ruleValues.Contains(ffUCProperty.Value))
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.StartsWith.ToString())
                        {
                            if (rule.Property == "KeyId" &&
                                ffUser.KeyId.StartsWith(rule.Value))
                                continue;
                            else if (rule.Property == "Name" &&
                                ffUser.Name.StartsWith(rule.Value))
                                continue;
                            else if (rule.Property == "Email" &&
                                ffUser.Email.StartsWith(rule.Value))
                                continue;
                            else if (rule.Property == ffUCProperty.Name &&
                                ffUCProperty.Value.StartsWith(rule.Value))
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.EndsWith.ToString())
                        {
                            if (rule.Property == "KeyId" &&
                                ffUser.KeyId.EndsWith(rule.Value))
                                continue;
                            else if (rule.Property == "Name" &&
                                ffUser.Name.EndsWith(rule.Value))
                                continue;
                            else if (rule.Property == "Email" &&
                                ffUser.Email.EndsWith(rule.Value))
                                continue;
                            else if (rule.Property == ffUCProperty.Name &&
                                ffUCProperty.Value.EndsWith(rule.Value))
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.IsTrue.ToString())
                        {
                            if (rule.Property == ffUCProperty.Name &&
                                (ffUCProperty.Value ?? "").ToUpper() == "TRUE")
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.IsFalse.ToString())
                        {
                            if (rule.Property == ffUCProperty.Name &&
                                (ffUCProperty.Value ?? "").ToUpper() == "FALSE")
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.MatchRegex.ToString())
                        {
                            Regex rgx = new Regex(rule.Value, RegexOptions.IgnoreCase);
                            MatchCollection matches = rgx.Matches(ffUCProperty.Value);
                            if (matches.Count > 0)
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                        else if (rule.Operation == RuleTypeEnum.NotMatchRegex.ToString())
                        {
                            Regex rgx = new Regex(rule.Value, RegexOptions.IgnoreCase);
                            MatchCollection matches = rgx.Matches(ffUCProperty.Value);
                            if (matches == null || matches.Count == 0)
                                continue;
                            else
                            {
                                isInCondition = false;
                                break;
                            }
                        }
                    }
                }
                else
                    isInCondition = false;

                if (isInCondition)
                {
                    foreach(var item in ffTUWMRItem.ValueOptionsVariationRuleValues)
                    {
                        if (IfBelongRolloutPercentage(ffUser.KeyId, item.RolloutPercentage))
                            return item.ValueOption;
                    }
                }
            }
            return null;
        }



        public bool IfBelongRolloutPercentage(string userFFKeyId, double[] rolloutPercentageRange)
        {
            byte[] hashedKey = new MD5CryptoServiceProvider().ComputeHash(ASCIIEncoding.ASCII.GetBytes(userFFKeyId));
            int a0 = BitConverter.ToInt32(hashedKey, 0);
            double y = Math.Abs((double)a0 / (double)int.MinValue);
            if (y >= rolloutPercentageRange[0] && y <= rolloutPercentageRange[1])
            {
                return true;
            }

            return false;
        }

        #endregion


        private async Task<EnvironmentUser> UpsertEnvironmentUserAsync(EnvironmentUser wsUser, int environmentId)
        {
            var oldWsUser = await _nosqlDbService.GetEnvironmentUserAsync(wsUser.Id);
            if (oldWsUser != null && !string.IsNullOrWhiteSpace(oldWsUser._Id))
            {
                wsUser._Id = oldWsUser._Id;
            }
            await _nosqlDbService.UpsertEnvironmentUserAsync(wsUser);
            return wsUser;
        }
    }

}
