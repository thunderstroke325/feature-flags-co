using FeatureFlags.APIs.Services;

namespace FeatureFlags.APIs.Models
{
    public abstract class UserVariation
    {
        public VariationOption Variation { get; }
        public abstract bool SendToExperiment { get; }

        protected UserVariation(VariationOption variation)
        {
            Variation = variation;
        }
    }

    public class CachedUserVariation : UserVariation
    {
        public override bool SendToExperiment { get; }

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
        public override bool SendToExperiment => false;

        public FeatureFlagDisabledUserVariation(VariationOption variation) : base(variation)
        {
        }
    }

    public class TargetedUserVariation : UserVariation
    {
        public override bool SendToExperiment { get; }

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
        public override bool SendToExperiment { get; }

        public ConditionedUserVariation(
            VariationOptionPercentageRollout percentageRollout,
            string userKey,
            bool? thisRuleIncludedInExperiment) : base(percentageRollout.ValueOption)
        {
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
        public override bool SendToExperiment { get; }

        public DefaultUserVariation(
            VariationOptionPercentageRollout percentageRollout,
            string userKey,
            bool? defaultRuleIncludedInExperiment) : base(percentageRollout.ValueOption)
        {
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