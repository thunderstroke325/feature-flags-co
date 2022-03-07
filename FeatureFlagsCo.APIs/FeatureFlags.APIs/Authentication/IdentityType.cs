namespace FeatureFlags.APIs.Authentication
{
    public enum IdentityType
    {
        None,
        Phone,
        Email
    }

    public class IdentityTypes
    {
        public static IdentityType Check(string identity)
        {
            // email contains @ character
            if (identity.Contains("@"))
            {
                return IdentityType.Email;
            }

            // china mobile phone number
            if (identity.Length == 11)
            {
                return IdentityType.Phone;
            }
            
            return IdentityType.None;
        }
    } 
}