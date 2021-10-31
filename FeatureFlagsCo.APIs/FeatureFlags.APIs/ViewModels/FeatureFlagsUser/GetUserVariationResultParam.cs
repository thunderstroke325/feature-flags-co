using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Models
{
    public class GetUserVariationResultParam
    {
        public string FeatureFlagKeyName { get; set; }
        public string EnvironmentSecret { get; set; }
        public string FFUserName { get; set; }
        public string FFUserEmail { get; set; }
        public string FFUserCountry { get; set; }
        public string FFUserKeyId { get; set; }
        public List<FeatureFlagUserCustomizedProperty> FFUserCustomizedProperties { get; set; }
    }


    public class GetUserVariationResultJsonViewModel
    {
        public bool? VariationResult { get; set; }
    }

    public class SendUserVariationViewModel : GetUserVariationResultParam 
    {
        public int VariationOptionId { get; set; }
    }
}
