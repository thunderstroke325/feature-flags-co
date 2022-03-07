namespace FeatureFlags.APIs.ViewModels.User
{
    public class SendIdentityCodeResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public static SendIdentityCodeResult Failed(string message)
        {
            var result = new SendIdentityCodeResult
            {
                Success = false,
                Message = message
            };

            return result;
        }
        
        public static SendIdentityCodeResult Ok()
        {
            var result = new SendIdentityCodeResult
            {
                Success = true,
                Message = string.Empty
            };

            return result;
        }
    }
}