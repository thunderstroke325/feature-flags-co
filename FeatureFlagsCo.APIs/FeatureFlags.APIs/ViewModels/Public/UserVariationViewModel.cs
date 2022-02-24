namespace FeatureFlags.APIs.ViewModels.Public
{
    public class UserVariationViewModel
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public string KeyName { get; set; }

        public string Variation { get; set; }

        public string Reason { get; set; }
    }
}