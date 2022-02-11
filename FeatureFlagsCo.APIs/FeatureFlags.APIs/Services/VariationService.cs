using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FeatureFlags.APIs.Repositories
{
    public interface IVariationService
    {
        Task<UserVariation> GetUserVariationAsync(
            EnvironmentUser user,
            FeatureFlagIdByEnvironmentKeyViewModel ffIdVm
        );
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

        public async Task<UserVariation> GetUserVariationAsync(
            EnvironmentUser user, 
            FeatureFlagIdByEnvironmentKeyViewModel ffIdVm)
        {
            string featureFlagId = ffIdVm.FeatureFlagId;

            int environmentId = Convert.ToInt32(ffIdVm.EnvId);
            user.EnvironmentId = environmentId;
            user.Id = FeatureFlagKeyExtension.GetEnvironmentUserId(environmentId, user.KeyId);

            // get feature flag info
            var featureFlagString = await _redisCache.GetStringAsync(featureFlagId);
            FeatureFlag featureFlag = null;
            if (!string.IsNullOrWhiteSpace(featureFlagString))
                featureFlag = JsonConvert.DeserializeObject<FeatureFlag>(featureFlagString);
            else
            {
                featureFlag = await _nosqlDbService.GetFeatureFlagAsync(featureFlagId);
                if (featureFlag == null) // feature flag doesn't exist
                {
                    return null;
                }

                await _redisCache.SetStringAsync(featureFlagId, JsonConvert.SerializeObject(featureFlag));
            }

            // get environment feature flag user info
            var featureFlagUserMappingId = FeatureFlagKeyExtension.GetFeatureFlagUserId(featureFlagId, user.KeyId);
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
                        if ((user.CustomizedProperties == userInfo.CustomizedProperties ||
                            JsonConvert.SerializeObject(user.CustomizedProperties).Trim() == JsonConvert.SerializeObject(userInfo.CustomizedProperties).Trim()) &&
                            user.Name == userInfo.Name)
                        {
                            return cosmosDBFeatureFlagsUser.CachedUserVariation();
                        }
                    }
                }
            }

            cosmosDBFeatureFlagsUser = await ReMatch(featureFlagId, featureFlagUserMappingId, featureFlag, user, ffIdVm);

            cosmosDBFeatureFlagsUser.UserInfo = user;
            await _redisCache.SetStringAsync(featureFlagUserMappingId, JsonConvert.SerializeObject(cosmosDBFeatureFlagsUser));

            await UpsertEnvironmentUserAsync(user);

            return cosmosDBFeatureFlagsUser.CachedUserVariation();
        }

        #region Need Refactor
        private async Task<EnvironmentFeatureFlagUser> ReMatch(
           string featureFlagId,
           string environmentFeatureFlagUserId,
           FeatureFlag featureFlag,
           EnvironmentUser environmentUser,
           FeatureFlagIdByEnvironmentKeyViewModel ffIdVm)
        {
            EnvironmentFeatureFlagUser environmentFeatureFlagUser = new EnvironmentFeatureFlagUser
                {
                    FeatureFlagId = featureFlagId,
                    EnvironmentId = environmentUser.EnvironmentId,
                    id = environmentFeatureFlagUserId
                };
            environmentFeatureFlagUser.LastUpdatedTime = DateTime.UtcNow;
            
            var userVariation = await MatchUserVariationAsync(featureFlag, environmentUser, ffIdVm);
            environmentFeatureFlagUser.VariationOptionResultValue = userVariation.Variation;
            environmentFeatureFlagUser.SendToExperiment = userVariation.SendToExperiment;
                
            return environmentFeatureFlagUser;
        }
        
        private async Task<UserVariation> MatchUserVariationAsync(
            FeatureFlag featureFlag, 
            EnvironmentUser user,
            FeatureFlagIdByEnvironmentKeyViewModel ffIdVm)
        {
            // 匹配该用户不满足上游开关设定值 或者 该开关已关闭状态下的 Variation
            var featureFlagDisabledUserVariation =
                await MatchFeatureFlagDisabledUserVariationAsync(featureFlag, user, ffIdVm);
            if (featureFlagDisabledUserVariation != null)
            {
                return featureFlagDisabledUserVariation;
            }

            // 匹配目标用户的 Variation
            var targetedUserVariation = MatchTargetedUserVariation(featureFlag, user);
            if (targetedUserVariation != null)
            {
                return targetedUserVariation;
            }

            // 匹配符合规则的用户的 Variation
            var conditionedUserVariation = MatchConditionedUserVariation(featureFlag, user);
            if (conditionedUserVariation != null)
            {
                return conditionedUserVariation;
            }
            
            // 匹配默认规则下的 Variation
            var defaultUserVariation = MatchDefaultUserVariation(featureFlag, user);
            if (defaultUserVariation != null)
            {
                return defaultUserVariation;
            }

            // 返回开关处于关闭状态时的结果
            return new FeatureFlagDisabledUserVariation(featureFlag.FF.VariationOptionWhenDisabled);
        }
        
        /// <summary>
        /// match feature flag disabled user variation
        /// </summary>
        /// <returns>if the feature flag is disabled or this user does not pass prerequisite check,
        /// returns FeatureFlagDisabledUserVariation instance, otherwise returns null</returns>
        private async Task<FeatureFlagDisabledUserVariation> MatchFeatureFlagDisabledUserVariationAsync(
            FeatureFlag featureFlag, 
            EnvironmentUser environmentUser,
            FeatureFlagIdByEnvironmentKeyViewModel ffIdVm)
        {
            // 判断开关是否处于关闭状态
            if (featureFlag.FF.Status == FeatureFlagStatutEnum.Disabled.ToString())
            {
                return new FeatureFlagDisabledUserVariation(featureFlag.FF.VariationOptionWhenDisabled);
            }

            // 判断是否满足上游开关的规则
            foreach (var ffPItem in featureFlag.FFP)
            {
                if (ffPItem.PrerequisiteFeatureFlagId != featureFlag.FF.Id)
                {
                    var r = await GetUserVariationAsync(
                        environmentUser,
                        new FeatureFlagIdByEnvironmentKeyViewModel()
                        {
                            AccountId = ffIdVm.AccountId,
                            EnvId = ffIdVm.EnvId,
                            FeatureFlagId = ffPItem.PrerequisiteFeatureFlagId,
                            ProjectId = ffIdVm.ProjectId
                        });
                    
                    // 若上游开关不符合条件 则认为此开关处于关闭状态
                    if (r.Variation.LocalId != ffPItem.ValueOptionsVariationValue.LocalId)
                    {
                        return new FeatureFlagDisabledUserVariation(featureFlag.FF.VariationOptionWhenDisabled);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// match targeted user variation
        /// </summary>
        /// <returns>if user is feature flag targeted, return TargetedUserVariation instance, otherwise return null</returns>
        private TargetedUserVariation MatchTargetedUserVariation(FeatureFlag featureFlag, EnvironmentUser user)
        {
            // 判断是否是指定的目标用户
            var targetedRules = featureFlag.TargetIndividuals;
            if (targetedRules == null || targetedRules.Count <= 0)
            {
                return null;
            }

            var targetedRule = targetedRules.FirstOrDefault(option => option.IsTargeted(user));
            return targetedRule != null
                ? new TargetedUserVariation(targetedRule.ValueOption, featureFlag.ExptIncludeAllRules)
                : null;
        }
        
        /// <summary>
        /// match conditioned user variation
        /// </summary>
        /// <returns>if user matched one of the feature flag rule, return ConditionedUserVariation, otherwise return null</returns>
        private ConditionedUserVariation MatchConditionedUserVariation(
          FeatureFlag cosmosDBFeatureFlag,
          EnvironmentUser ffUser)
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
                        var userKey = ffUser.KeyId;
                        if (IfBelongRolloutPercentage(userKey, item.RolloutPercentage))
                        {
                            var newUserKey = userKey.EncodeBase64();
                            return new ConditionedUserVariation(item, newUserKey, ffTUWMRItem.isIncludedInExpt);
                        }
                    }
                }
            }
            return null;
        }
        
        /// <summary>
        /// match default user variation
        /// </summary>
        /// <returns>if user match default rule, return DefaultUserVariation, otherwise return null</returns>
        private DefaultUserVariation MatchDefaultUserVariation(FeatureFlag featureFlag, EnvironmentUser user)
        {
            var defaultRollouts = featureFlag.FF.DefaultRulePercentageRollouts;
            if (defaultRollouts == null)
            {
                return null;
            }
            
            var userKey = user.KeyId;
            var matchedRollout = defaultRollouts.FirstOrDefault(
                rollout => IfBelongRolloutPercentage(userKey, rollout.RolloutPercentage)
            );
            
            var newUserKey = userKey.EncodeBase64();
            return matchedRollout != null
                ? new DefaultUserVariation(matchedRollout, newUserKey, featureFlag.FF.IsDefaultRulePercentageRolloutsIncludedInExpt) 
                : null;
        }

        private async Task UpsertEnvironmentUserAsync(EnvironmentUser wsUser)
        {
            var oldWsUser = await _nosqlDbService.GetEnvironmentUserAsync(wsUser.Id);
            if (oldWsUser != null && !string.IsNullOrWhiteSpace(oldWsUser._Id))
            {
                wsUser._Id = oldWsUser._Id;
            }
            
            await _nosqlDbService.UpsertEnvironmentUserAsync(wsUser);
        }

        private bool IfBelongRolloutPercentage(string key, double[] percentageRange)
            => VariationSplittingAlgorithm.IfKeyBelongsPercentage(key, percentageRange);
        
        #endregion
    }

}
