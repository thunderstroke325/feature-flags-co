using FeatureFlags.APIs.Services;

namespace FeatureFlags.APIs.Models
{
    public abstract class UserVariation
    {
        public VariationOption Variation { get; set; }
        public abstract bool SendToExperiment { get; set; }

        protected UserVariation(VariationOption variation)
        {
            Variation = variation;
        }

        protected UserVariation()
        {
        }
    }

    public class InsightUserVariation: UserVariation
    {
        public string FeatureFlagKeyName { get; set; }
        public override bool SendToExperiment { get; set;  }
        public long Timestamp { get; set; }
        public InsightUserVariation(VariationOption variation) : base(variation)
        {
        }

        public InsightUserVariation()
        {
        }
    }

    public class CachedUserVariation : UserVariation
    {
        public override bool SendToExperiment { get; set; }

        public CachedUserVariation(VariationOption variation, bool? sendToExperiment)
            : base(variation)
        {
            SendToExperiment =
                // if sendToExperiment is null, that means send all data to experiment
                !sendToExperiment.HasValue ||
                sendToExperiment.Value;
        }
    }

    public class FeatureFlagDisabledUserVariation : UserVariation
    {
        public override bool SendToExperiment { get; set; } = false;

        public FeatureFlagDisabledUserVariation(VariationOption variation) : base(variation)
        {
        }
    }

    public class TargetedUserVariation : UserVariation
    {
        public override bool SendToExperiment { get; set; }

        public TargetedUserVariation(
            VariationOption variation,
            bool? experimentIncludeAllRules) : base(variation)
        {
            SendToExperiment =
                // if experimentIncludeAllRules is null, that means send all data to experiment
                !experimentIncludeAllRules.HasValue ||
                experimentIncludeAllRules.Value;
        }
    }

    public class ConditionedUserVariation : UserVariation
    {
        public override bool SendToExperiment { get; set; }

        public ConditionedUserVariation(
            VariationOptionPercentageRollout percentageRollout,
            string userKey,
            bool? includeAllInExperiment, 
            bool? thisRuleIncludedInExperiment) : base(percentageRollout.ValueOption)
        {
            // if includeAllInExperiment is null or true, that means send all data to experiment
            if (!includeAllInExperiment.HasValue || includeAllInExperiment.Value)
            {
                SendToExperiment = true;
                return;
            }
            
            // if thisRuleIncludedInExperiment or this rule's experiment rollout is null
            // that means send all data to experiment
            if (!thisRuleIncludedInExperiment.HasValue || !percentageRollout.ExptRollout.HasValue)
            {
                SendToExperiment = true;
                return;
            }

            // this rule does not participate in experiment
            if (!thisRuleIncludedInExperiment.Value)
            {
                SendToExperiment = false;
                return;
            }

            // send to experiment by percentage
            var sendToExperimentPercentage = percentageRollout.ExptRollout.Value;
            var splittingPercentage = percentageRollout.RolloutPercentage[1] - percentageRollout.RolloutPercentage[0];
            if (sendToExperimentPercentage == 0.0 || splittingPercentage == 0.0)
            {
                SendToExperiment = false;
                return;
            }
            
            var upperBound = sendToExperimentPercentage / splittingPercentage;
            if (upperBound > 1.0)
            {
                upperBound = 1.0;
            }
            SendToExperiment = VariationSplittingAlgorithm.IfKeyBelongsPercentage(userKey, new[] { 0.0, upperBound });
        }
    }

    public class DefaultUserVariation : UserVariation
    {
        public override bool SendToExperiment { get; set; }

        public DefaultUserVariation(
            VariationOptionPercentageRollout percentageRollout,
            string userKey,
            bool? includeAllInExperiment, 
            bool? defaultRuleIncludedInExperiment) : base(percentageRollout.ValueOption)
        {
            // if includeAllInExperiment is null or true, that means send all data to experiment
            if (!includeAllInExperiment.HasValue || includeAllInExperiment.Value)
            {
                SendToExperiment = true;
                return;
            }
            
            // if defaultRuleIncludedInExperiment is null or the default rule's experiment rollout is null, 
            // that means send all data to experiment
            if (!defaultRuleIncludedInExperiment.HasValue || !percentageRollout.ExptRollout.HasValue)
            {
                SendToExperiment = true;
                return;
            }

            // default rule does not participate in experiment
            if (!defaultRuleIncludedInExperiment.Value)
            {
                SendToExperiment = false;
                return;
            }

            // send to experiment by percentage
            var sendToExperimentPercentage = percentageRollout.ExptRollout.Value;
            var splittingPercentage = percentageRollout.RolloutPercentage[1] - percentageRollout.RolloutPercentage[0];
            if (sendToExperimentPercentage == 0.0 || splittingPercentage == 0.0)
            {
                SendToExperiment = false;
                return;
            }
            
            var upperBound = sendToExperimentPercentage / splittingPercentage;
            if (upperBound > 1.0)
            {
                upperBound = 1.0;
            }
            SendToExperiment = VariationSplittingAlgorithm.IfKeyBelongsPercentage(userKey, new[] { 0.0, upperBound });
        }
    }
}