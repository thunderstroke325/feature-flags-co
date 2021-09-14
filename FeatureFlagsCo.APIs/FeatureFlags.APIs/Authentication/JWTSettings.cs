namespace FeatureFlags.APIs.Authentication
{
    public class JWTSettings
    {
        public string ValidAudience { get; set; }

        public string ValidIssuer { get; set; }

        public string Secret { get; set; }
    }
}