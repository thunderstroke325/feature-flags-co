namespace FeatureFlags.APIs.Models
{
    public class AccountUserDetailV2
    {
        public string UserId { get; set; }
        
        public string UserName { get; set; }
        
        public string Email { get; set; }
        
        public string Role { get; set; }

        public string InvitorUserId { get; set; }
        
        public string InitialPassword { get; set; }
    }
}