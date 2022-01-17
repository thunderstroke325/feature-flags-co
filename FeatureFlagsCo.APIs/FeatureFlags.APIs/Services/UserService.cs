using System;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace FeatureFlags.APIs.Services
{
    public class UserService : ITransientDependency
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApplicationUser> CreateAsync(
            string userName, 
            string email, 
            string password, 
            bool emailConfirmed = true) 
        {
            var user = new ApplicationUser
            {
                Email = userName,
                UserName = email,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = emailConfirmed
            };
            
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new BusinessException($"create user {result}");
            }

            return user;
        }
        
        public async Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
    }
}