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

namespace FeatureFlags.APIs.Repositories
{
    public interface IVariationService
    {
        Task<Tuple<bool?, bool>> CheckVariableAsync(string environmentSecret, string featureFlagKeyName, CosmosDBEnvironmentUser ffUser,
            FeatureFlagIdByEnvironmentKeyViewModel ffIdVM);
    }

    public class VariationService : IVariationService
    {
        private readonly IDistributedCache _redisCache;
        private readonly ICosmosDbService _cosmosDbService;

        public VariationService(
            IGenericRepository repository,
            IDistributedCache redisCache,
            ICosmosDbService cosmosDbService)
        {
            _redisCache = redisCache;
            _cosmosDbService = cosmosDbService;
        }

        public async Task<Tuple<bool?, bool>> CheckVariableAsync(string environmentSecret, string featureFlagKeyName, CosmosDBEnvironmentUser environmentUser,
            FeatureFlagIdByEnvironmentKeyViewModel ffIdVM)
        {
            string featureFlagId = ffIdVM.FeatureFlagId;

            int environmentId = Convert.ToInt32(ffIdVM.EnvId);
            environmentUser.EnvironmentId = environmentId;
            environmentUser.id = FeatureFlagKeyExtension.GetEnvironmentUserId(environmentId, environmentUser.KeyId);

            bool readOnlyOperation = true;

            // get feature flag info
            var featureFlagString = await _redisCache.GetStringAsync(featureFlagId);
            CosmosDBFeatureFlag featureFlag = null;
            if (!string.IsNullOrWhiteSpace(featureFlagString))
                featureFlag = JsonConvert.DeserializeObject<CosmosDBFeatureFlag>(featureFlagString);
            else
            {
                featureFlag = await _cosmosDbService.GetFeatureFlagAsync(featureFlagId);
                await _redisCache.SetStringAsync(featureFlagId, JsonConvert.SerializeObject(featureFlag));
                readOnlyOperation = false;
            }

            // get environment feature flag user info
            var featureFlagUserMappingId = FeatureFlagKeyExtension.GetFeatureFlagUserId(featureFlagId, environmentUser.KeyId);
            var featureFlagsUserMappingString = await _redisCache.GetStringAsync(featureFlagUserMappingId);
            CosmosDBEnvironmentFeatureFlagUser cosmosDBFeatureFlagsUser = null;

            if (!string.IsNullOrWhiteSpace(featureFlagsUserMappingString))
            {
                cosmosDBFeatureFlagsUser = JsonConvert.DeserializeObject<CosmosDBEnvironmentFeatureFlagUser>(featureFlagsUserMappingString);
                if (featureFlag.FF.LastUpdatedTime == null || cosmosDBFeatureFlagsUser.LastUpdatedTime == null ||
                featureFlag.FF.LastUpdatedTime.Value.CompareTo(cosmosDBFeatureFlagsUser.LastUpdatedTime.Value) > 0)
                {
                    // comosdb 4000 - 处理425个请求 / 秒 - 在RedoMatchingAndUpdateToRedisCacheAsync之前
                    cosmosDBFeatureFlagsUser = await RedoVariationServiceAsync(
                                                        environmentUser,
                                                        featureFlagId,
                                                        featureFlagUserMappingId,
                                                        featureFlag);
                    readOnlyOperation = false;
                }
                return new Tuple<bool?, bool>(cosmosDBFeatureFlagsUser.ResultValue ?? false, readOnlyOperation);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(featureFlagString))
                {
                    featureFlag = await _cosmosDbService.GetFeatureFlagAsync(featureFlagId);
                    await _redisCache.SetStringAsync(featureFlagId, JsonConvert.SerializeObject(featureFlag));
                }

                cosmosDBFeatureFlagsUser = await RedoVariationServiceAsync(
                                                    environmentUser,
                                                    featureFlagId,
                                                    featureFlagUserMappingId,
                                                    featureFlag);
            }

            // comosdb 4000 - 处理243个请求 / 秒 - 在AddOrUpdateEnvironmentUserIntoDatabaseAsync之前

            await UpsertEnvironmentUserAsync(environmentUser, environmentId);

            return new Tuple<bool?, bool>(cosmosDBFeatureFlagsUser.ResultValue, false);
        }

        public async Task<CosmosDBEnvironmentFeatureFlagUser> RedoVariationServiceAsync(
            CosmosDBEnvironmentUser environmentUser,
            string featureFlagId,
            string featureFlagUserMappingId,
            CosmosDBFeatureFlag featureFlag)
        {
            CosmosDBEnvironmentFeatureFlagUser cosmosDBFeatureFlagsUser = await RedoMatchingAndUpdateToRedisCacheAsync(
                featureFlagId, featureFlagUserMappingId, featureFlag, environmentUser);

            await _cosmosDbService.UpdateCosmosDBFeatureFlagAsync(featureFlag);

            return cosmosDBFeatureFlagsUser;
        }

        private async Task<CosmosDBEnvironmentFeatureFlagUser> RedoMatchingAndUpdateToRedisCacheAsync(
            string featureFlagId,
            string environmentFeatureFlagUserId,
            CosmosDBFeatureFlag featureFlag,
            CosmosDBEnvironmentUser environmentUser)
        {
            CosmosDBEnvironmentFeatureFlagUser environmentFeatureFlagUser = await _cosmosDbService.GetEnvironmentFeatureFlagUserAsync(environmentFeatureFlagUserId);
            if (environmentFeatureFlagUser == null)
            {
                environmentFeatureFlagUser = new CosmosDBEnvironmentFeatureFlagUser
                {
                    FeatureFlagId = featureFlagId,
                    EnvironmentId = environmentUser.EnvironmentId,
                    id = environmentFeatureFlagUserId
                };
                environmentFeatureFlagUser.LastUpdatedTime = DateTime.UtcNow;
                environmentFeatureFlagUser.ResultValue = await GetUserVariationResultAsync(featureFlag, environmentUser, environmentFeatureFlagUser);
                await _cosmosDbService.AddCosmosDBEnvironmentFeatureFlagUserAsync(environmentFeatureFlagUser);
            }
            else
            {
                environmentFeatureFlagUser.LastUpdatedTime = DateTime.UtcNow;
                environmentFeatureFlagUser.ResultValue = await GetUserVariationResultAsync(featureFlag, environmentUser, environmentFeatureFlagUser);
                await _cosmosDbService.UpdateItemAsync(environmentFeatureFlagUser.id, environmentFeatureFlagUser);
            }

            await _redisCache.SetStringAsync(environmentFeatureFlagUserId, JsonConvert.SerializeObject(environmentFeatureFlagUser));
            return environmentFeatureFlagUser;
        }

        private async Task<CosmosDBEnvironmentUser> UpsertEnvironmentUserAsync(CosmosDBEnvironmentUser wsUser, int environmentId)
        {
            await _cosmosDbService.UpsertEnvironmentUserAsync(wsUser);
            return wsUser;
        }

        public async Task<bool?> GetUserVariationResultAsync(CosmosDBFeatureFlag cosmosDBFeatureFlag, CosmosDBEnvironmentUser featureFlagsUser,
            CosmosDBEnvironmentFeatureFlagUser environmentFeatureFlagUser)
        {
            var wsId = cosmosDBFeatureFlag.EnvironmentId;
            //var environment = await _dbContext.Environments.FirstOrDefaultAsync(p => p.Id == wsId);

            if (cosmosDBFeatureFlag.FF.Status == FeatureFlagStatutEnum.Disabled.ToString())
                return cosmosDBFeatureFlag.FF.ValueWhenDisabled ?? false;

            // 判断Prequisite
            foreach (var ffPItem in cosmosDBFeatureFlag.FFP)
            {
                if (ffPItem.PrerequisiteFeatureFlagId != cosmosDBFeatureFlag.FF.Id)
                {
                    var newFF = await _cosmosDbService.GetItemAsync(ffPItem.PrerequisiteFeatureFlagId);
                    var ffPResult = await GetUserVariationResultAsync(newFF, featureFlagsUser, environmentFeatureFlagUser);
                    if (ffPResult.Value != ffPItem.VariationValue)
                        return cosmosDBFeatureFlag.FF.ValueWhenDisabled ?? false;
                }
            }

            // 判断Individual Rule
            if (cosmosDBFeatureFlag.FFTIUForFalse != null && cosmosDBFeatureFlag.FFTIUForFalse.Count > 0 &&
                cosmosDBFeatureFlag.FFTIUForFalse.Any(p => p.KeyId == featureFlagsUser.KeyId))
                return false;
            if (cosmosDBFeatureFlag.FFTIUForTrue != null && cosmosDBFeatureFlag.FFTIUForTrue.Count > 0 &&
                cosmosDBFeatureFlag.FFTIUForTrue.Any(p => p.KeyId == featureFlagsUser.KeyId))
                return true;

            // 判断Match Rules
            var ffUserCustomizedProperties = featureFlagsUser.CustomizedProperties ?? new List<FeatureFlagUserCustomizedProperty>();
            var ffTargetUsersWhoMatchRules = cosmosDBFeatureFlag.FFTUWMTR ?? new List<CosmosDBFeatureFlagTargetUsersWhoMatchTheseRuleParam>();
            var ruleMatchResult = GetUserVariationRuleMatchResult(
                cosmosDBFeatureFlag,
                featureFlagsUser, 
                environmentFeatureFlagUser);
            if (ruleMatchResult != null)
                return ruleMatchResult;

            // 判断Default Rule
            if (cosmosDBFeatureFlag.FF.PercentageRolloutForFalse != null &&
                cosmosDBFeatureFlag.FF.PercentageRolloutForTrue != null)
            {
                return GetUserVariationDefaultRulePercentageResult(cosmosDBFeatureFlag, featureFlagsUser, environmentFeatureFlagUser);
            }
            if (cosmosDBFeatureFlag.FF.DefaultRuleValue != null)
                return cosmosDBFeatureFlag.FF.DefaultRuleValue;


            return false;
        }

        private bool? GetUserVariationRuleMatchResult(
            CosmosDBFeatureFlag cosmosDBFeatureFlag,
            CosmosDBEnvironmentUser ffUser, 
            CosmosDBEnvironmentFeatureFlagUser environmentFeatureFlagUser)
        {
            foreach (var ffTUWMRItem in cosmosDBFeatureFlag.FFTUWMTR)
            {

                var rules = ffTUWMRItem.RuleJsonContent;
                bool isInCondition = true;
                if (rules != null && rules.Count > 0)
                {
                    foreach (var rule in rules)
                    {
                        var ffUCProperty = ffUser.CustomizedProperties.FirstOrDefault(p => p.Name == rule.Property);
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
                    if(ffTUWMRItem.PercentageRolloutForFalse  != null && ffTUWMRItem.PercentageRolloutForTrue != null)
                    {
                        string ffuKeyId = ffUser.KeyId;

                        if (ffTUWMRItem.PercentageRolloutForTrue > 1)
                            ffTUWMRItem.PercentageRolloutForTrue = ffTUWMRItem.PercentageRolloutForTrue / 100;
                        if (ffTUWMRItem.PercentageRolloutForFalse > 1)
                            ffTUWMRItem.PercentageRolloutForFalse = ffTUWMRItem.PercentageRolloutForFalse / 100;

                        //if (environmentFeatureFlagUser.ResultValue == null || environmentFeatureFlagUser.PercentageRolloutBasedRuleId != ffTUWMRItem.RuleId)
                        //{
                        environmentFeatureFlagUser.PercentageRolloutBasedRuleId = ffTUWMRItem.RuleId;
                        int trueUserCount = ffTUWMRItem.PercentageRolloutForTrueNumber;
                        int falseUserCount = ffTUWMRItem.PercentageRolloutForFalseNumber;

                        if (ffTUWMRItem.PercentageRolloutForTrue >= 1)
                        {
                            SetPercentageAndResultWhenTrue(environmentFeatureFlagUser, ffTUWMRItem);
                        }
                        else if (ffTUWMRItem.PercentageRolloutForFalse >= 1)
                        {
                            SetPercentageAndResultWhenFalse(environmentFeatureFlagUser, ffTUWMRItem);
                            //environmentFeatureFlagUser.ResultValue = false;
                            //ffTUWMRItem.PercentageRolloutForFalseNumber++;
                        }
                        else if ((ffTUWMRItem.PercentageRolloutForTrue ?? 0) < (ffTUWMRItem.PercentageRolloutForFalse ?? 0))
                        {
                            if (trueUserCount == 0 && falseUserCount == 0 && ffTUWMRItem.PercentageRolloutForTrue != 0)
                            {
                                SetPercentageAndResultWhenTrue(environmentFeatureFlagUser, ffTUWMRItem);
                                //environmentFeatureFlagUser.ResultValue = true;
                                //ffTUWMRItem.PercentageRolloutForTrueNumber++;
                            }
                            else if (((double)trueUserCount / ((double)trueUserCount + (double)falseUserCount)) <= ffTUWMRItem.PercentageRolloutForTrue)
                            {
                                SetPercentageAndResultWhenTrue(environmentFeatureFlagUser, ffTUWMRItem);
                                //environmentFeatureFlagUser.ResultValue = true;
                                //ffTUWMRItem.PercentageRolloutForTrueNumber++;
                            }
                            else
                            {
                                SetPercentageAndResultWhenFalse(environmentFeatureFlagUser, ffTUWMRItem);
                                //environmentFeatureFlagUser.ResultValue = false;
                                //ffTUWMRItem.PercentageRolloutForFalseNumber++;
                            }
                        }
                        else
                        {
                            if (trueUserCount == 0 && falseUserCount == 0 && ffTUWMRItem.PercentageRolloutForFalse != 0)
                            {
                                SetPercentageAndResultWhenFalse(environmentFeatureFlagUser, ffTUWMRItem);
                                //environmentFeatureFlagUser.ResultValue = false;
                                //ffTUWMRItem.PercentageRolloutForFalseNumber++;
                            }
                            else if (((double)falseUserCount / ((double)trueUserCount + (double)falseUserCount)) <= ffTUWMRItem.PercentageRolloutForFalse)
                            {
                                SetPercentageAndResultWhenFalse(environmentFeatureFlagUser, ffTUWMRItem);
                                //environmentFeatureFlagUser.ResultValue = false;
                                //ffTUWMRItem.PercentageRolloutForFalseNumber++;
                            }
                            else
                            {
                                SetPercentageAndResultWhenTrue(environmentFeatureFlagUser, ffTUWMRItem);
                                //environmentFeatureFlagUser.ResultValue = true;
                                //ffTUWMRItem.PercentageRolloutForTrueNumber++;
                            }
                        }
                        //}

                        return environmentFeatureFlagUser.ResultValue;
                    }
                    else
                    {
                        return ffTUWMRItem.VariationRuleValue;
                    }
                }
            }
            return null;
        }

        private void SetPercentageAndResultWhenTrue(CosmosDBEnvironmentFeatureFlagUser environmentFeatureFlagUser, CosmosDBFeatureFlagTargetUsersWhoMatchTheseRuleParam ffTUWMRItem)
        {
            if (environmentFeatureFlagUser.ResultValue == false &&
                environmentFeatureFlagUser.PercentageRolloutBasedRuleId == ffTUWMRItem.RuleId)
            {
                environmentFeatureFlagUser.ResultValue = true;
                ffTUWMRItem.PercentageRolloutForTrueNumber++;
                ffTUWMRItem.PercentageRolloutForFalseNumber--;
                if (ffTUWMRItem.PercentageRolloutForFalseNumber < 0)
                    ffTUWMRItem.PercentageRolloutForFalseNumber = 0;
            }
            else if (environmentFeatureFlagUser.ResultValue == true &&
                environmentFeatureFlagUser.PercentageRolloutBasedRuleId == ffTUWMRItem.RuleId)
            {
                //什么都不做
            }
            else
            {
                environmentFeatureFlagUser.ResultValue = true;
                ffTUWMRItem.PercentageRolloutForTrueNumber++;
            }
        }

        private void SetPercentageAndResultWhenFalse(CosmosDBEnvironmentFeatureFlagUser environmentFeatureFlagUser, CosmosDBFeatureFlagTargetUsersWhoMatchTheseRuleParam ffTUWMRItem)
        {
            if (environmentFeatureFlagUser.ResultValue == true &&
                environmentFeatureFlagUser.PercentageRolloutBasedRuleId == ffTUWMRItem.RuleId)
            {
                environmentFeatureFlagUser.ResultValue = false;
                ffTUWMRItem.PercentageRolloutForTrueNumber--;
                if (ffTUWMRItem.PercentageRolloutForTrueNumber < 0)
                    ffTUWMRItem.PercentageRolloutForTrueNumber = 0;
                ffTUWMRItem.PercentageRolloutForFalseNumber++;
            }
            else if (environmentFeatureFlagUser.ResultValue == false &&
                environmentFeatureFlagUser.PercentageRolloutBasedRuleId == ffTUWMRItem.RuleId)
            {
                //什么都不做
            }
            else
            {
                environmentFeatureFlagUser.ResultValue = false;
                ffTUWMRItem.PercentageRolloutForFalseNumber++;
            }
        }

        private bool? GetUserVariationDefaultRulePercentageResult(
            CosmosDBFeatureFlag ffParam, CosmosDBEnvironmentUser featureFlagsUser,
            CosmosDBEnvironmentFeatureFlagUser environmentFeatureFlagUser)
        {
            if (ffParam.FF.PercentageRolloutForFalse != null && ffParam.FF.PercentageRolloutForTrue != null)
            {
                string ffuKeyId = featureFlagsUser.KeyId;

                if (ffParam.FF.PercentageRolloutForTrue > 1)
                    ffParam.FF.PercentageRolloutForTrue = ffParam.FF.PercentageRolloutForTrue / 100;
                if (ffParam.FF.PercentageRolloutForFalse > 1)
                    ffParam.FF.PercentageRolloutForFalse = ffParam.FF.PercentageRolloutForFalse / 100;

                if (environmentFeatureFlagUser.ResultValue == null || environmentFeatureFlagUser.PercentageRolloutBasedRuleId != ffParam.Id)
                {
                    environmentFeatureFlagUser.PercentageRolloutBasedRuleId = ffParam.Id;
                    int trueUserCount = ffParam.FF.PercentageRolloutForTrueNumber;
                    int falseUserCount = ffParam.FF.PercentageRolloutForFalseNumber;
                    if ((ffParam.FF.PercentageRolloutForTrue ?? 0) < (ffParam.FF.PercentageRolloutForFalse ?? 0))
                    {
                        if (trueUserCount == 0 && falseUserCount == 0 && ffParam.FF.PercentageRolloutForTrue != 0)
                        {
                            environmentFeatureFlagUser.ResultValue = true;
                            ffParam.FF.PercentageRolloutForTrueNumber++;
                        }
                        else if (((double)trueUserCount / ((double)trueUserCount + (double)falseUserCount)) <= ffParam.FF.PercentageRolloutForTrue)
                        {
                            environmentFeatureFlagUser.ResultValue = true;
                            ffParam.FF.PercentageRolloutForTrueNumber++;
                        }
                        else
                        {
                            environmentFeatureFlagUser.ResultValue = false;
                            ffParam.FF.PercentageRolloutForFalseNumber++;
                        }
                    }
                    else
                    {
                        if (trueUserCount == 0 && falseUserCount == 0 && ffParam.FF.PercentageRolloutForFalse != 0)
                        {
                            environmentFeatureFlagUser.ResultValue = false;
                            ffParam.FF.PercentageRolloutForFalseNumber++;
                        }
                        else if (((double)falseUserCount / ((double)trueUserCount + (double)falseUserCount)) <= ffParam.FF.PercentageRolloutForFalse)
                        {
                            environmentFeatureFlagUser.ResultValue = false;
                            ffParam.FF.PercentageRolloutForFalseNumber++;
                        }
                        else
                        {
                            environmentFeatureFlagUser.ResultValue = true;
                            ffParam.FF.PercentageRolloutForTrueNumber++;
                        }
                    }
                }

                return environmentFeatureFlagUser.ResultValue;
            }
            else
            {
                return ffParam.FF.DefaultRuleValue;
            }
        }
    }

}
