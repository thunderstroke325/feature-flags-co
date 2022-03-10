using System;
using Authing.ApiClient.Types;
using Microsoft.AspNetCore.Identity;

namespace FeatureFlags.APIs.Authentication
{
    public class ApplicationUser: IdentityUser
    {
        public ApplicationUser()
        {
            
        }
        
        public ApplicationUser(User authingUser)
        {
            var identity = authingUser.Email ?? authingUser.Phone;
            
            Id = authingUser.Id;
            UserName = identity;
            Email = authingUser.Email;
            PhoneNumber = authingUser.Phone;
            SecurityStamp = Guid.NewGuid().ToString();
        }
    }

    public class UserRegistrationException : Exception
    {
        public UserRegistrationException(string message) : base(message)
        {
        }
    }

    public class SendIdentityCodeException : Exception
    {
        public SendIdentityCodeException(string message) : base(message)
        {
        }
    }
}
