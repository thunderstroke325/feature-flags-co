using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.Utils.ConventionalDependencyInjection;

namespace FeatureFlags.APIs.Services
{
    public class AccountV2AppService : ITransientDependency
    {
        private readonly AccountV2Service _accountService;
        private readonly ProjectV2AppService _projectAppService;
        private readonly UserService _userService;

        public AccountV2AppService(
            AccountV2Service accountService,
            ProjectV2AppService projectAppService,
            UserService userService)
        {
            _accountService = accountService;
            _userService = userService;
            _projectAppService = projectAppService;
        }

        public async Task<AccountV2> CreateAsync(
            string organizationName, 
            string creatorId, 
            bool createDefaultFeatureFlag = false)
        {
            // create account and account user
            var account = await _accountService.CreateAsync(organizationName, creatorId);
            
            // create default project for new account
            await _projectAppService.CreateAsync(account.Id, "Default Project", creatorId, createDefaultFeatureFlag);

            return account;
        }

        public async Task<bool> DeleteAsync(int accountId)
        {
            // delete account and account users
            await _accountService.DeleteAsync(accountId);

            // delete account projects
            await _projectAppService.DeleteAsync(project => project.AccountId == accountId);

            return true;
        }

        public async Task<bool> InviteUserAsync(
            int accountId,
            string email,
            string role,
            string invitorId)
        {
            string initialPassword = null;
            
            var user = await _userService.FindByEmailAsync(email);
            if (user == null)
            {
                // create a new user
                initialPassword = MiscService.GeneratePassword(email);
                user = await _userService.CreateAsync(email, email, initialPassword);

                // create default account for user
                await CreateAsync("Default Organization", user.Id);
            }

            // create account user
            await _accountService.CreateUserAsync(accountId, user.Id, role, invitorId, initialPassword);

            return true;
        }
    }
}