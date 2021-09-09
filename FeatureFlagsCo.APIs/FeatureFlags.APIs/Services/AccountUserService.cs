using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels.Account;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FeatureFlags.APIs.Services
{
    public interface IAccountUserService
    {
        // Remove all users of an account
        // The currentUser must be the owner of the account, must be checked before calling this method
        public Task RemoveAllUsersAsync(int accountId);

        // Remove a specific user of an account
        // The currentUser must be the owner or admin of the account, must be checked before calling this method
        public Task RemoveUserAsync(int accountId, string userId);

        public Task<AccountUserMapping> GetAccountOwnerAsync(int accountId);

        Task<List<AccountUserViewModel>> SearchAccountMemberAsync(int accountId, string searchText, int returnSize = 5);

        public Task<AccountUserMapping> GetOwnerAccountAsync(string userId);

        public bool IsInAccountUserRoles(int accountId, string userId, IEnumerable<AccountUserRoleEnum> roles);

        public bool IsAccountMember(int accountId, string userId);

        public Task<List<AccountUserViewModel>> GetAccountMembersAsync(string currentUserId, int accountId);

    }

    public class AccountUserService : IAccountUserService
    {
        private readonly ApplicationDbContext _dbContext;

        public AccountUserService(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public async Task<AccountUserMapping> GetAccountOwnerAsync(int accountId)
        {
            return await _dbContext
                .AccountUserMappings
                .FirstOrDefaultAsync(x => x.AccountId == accountId && x.Role == AccountUserRoleEnum.Owner.ToString());
        }

        public async Task<AccountUserMapping> GetOwnerAccountAsync(string userId) 
        {
            return await _dbContext
                    .AccountUserMappings
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.Role == AccountUserRoleEnum.Owner.ToString());
        }

        public async Task<List<AccountUserViewModel>> GetAccountMembersAsync(string currentUserId, int accountId)
        {
            var includeInitialPassword = IsInAccountUserRoles(accountId, currentUserId, new List<AccountUserRoleEnum> { AccountUserRoleEnum.Owner });

            IQueryable<AccountUserViewModel> query = from aum in _dbContext.AccountUserMappings
                                                     join user in _dbContext.Users on aum.UserId equals user.Id // inner join
                                                     join invit in _dbContext.UserInvitations on user.Id equals invit.UserId into invits
                                                     from invit in invits.DefaultIfEmpty() // left join
                                                     where aum.AccountId == accountId
                                                     select new AccountUserViewModel
                                                     {
                                                         UserId = user.Id,
                                                         UserName = user.UserName,
                                                         Email = user.Email,
                                                         Role = aum.Role,
                                                         InitialPassword = includeInitialPassword || currentUserId.Equals(aum.InvitorUserId) ? invit.InitialPassword : null
                                                     };

            return await query.ToListAsync();
        }

        public async Task<List<AccountUserViewModel>> SearchAccountMemberAsync(int accountId, string searchText, int returnSize = 5)
        {
            IQueryable<AccountUserViewModel> query = (from aum in _dbContext.AccountUserMappings
                                                     join user in _dbContext.Users on aum.UserId equals user.Id // inner join
                                                     where aum.AccountId == accountId && (user.Email.Contains(searchText) || user.UserName.Contains(searchText))
                                                     select new AccountUserViewModel
                                                     {
                                                         UserId = user.Id,
                                                         Email = user.Email,
                                                         UserName = user.UserName,
                                                         Role = aum.Role,
                                                     }).Take(returnSize);

            return await query.ToListAsync();
        }

        public async Task RemoveAllUsersAsync(int accountId)
        {
            var aums = _dbContext.AccountUserMappings.Where(p => p.AccountId == accountId).ToList();
            if (aums != null && aums.Count > 0)
            {
                foreach (var item in aums)
                {
                    _dbContext.AccountUserMappings.Remove(item);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public bool IsInAccountUserRoles(int accountId, string userId, IEnumerable<AccountUserRoleEnum> roles) 
        {
            var roleStrings = roles.Select(x => x.ToString());
            var accountUser = _dbContext
                .AccountUserMappings
                .Where(p => p.AccountId == accountId && p.UserId == userId && roleStrings.Contains(p.Role))
                .FirstOrDefault();

            return accountUser != null;
        }

        public async Task RemoveUserAsync(int accountId, string userId)
        {
            var aum = _dbContext.AccountUserMappings.Where(p => p.AccountId == accountId && p.UserId == userId).FirstOrDefault();
            if (aum != null)
            {
                _dbContext.AccountUserMappings.Remove(aum);
            }

            await _dbContext.SaveChangesAsync();
        }

        public bool IsAccountMember(int accountId, string userId)
        {
            var accountUser = _dbContext
                .AccountUserMappings
                .Where(p => p.AccountId == accountId && p.UserId == userId)
                .FirstOrDefault();

            return accountUser != null;
        }
    }
}
