using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.APIs.ViewModels.Account;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.Exceptions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Services
{
    public class AccountV2Service : ITransientDependency
    {
        private readonly MongoDbIntIdRepository<AccountV2> _accounts;
        private readonly MongoDbIntIdRepository<AccountUserV2> _accountUsers;

        public AccountV2Service(
            MongoDbIntIdRepository<AccountV2> accounts,
            MongoDbIntIdRepository<AccountUserV2> accountUsers)
        {
            _accounts = accounts;
            _accountUsers = accountUsers;
        }

        public async Task<AccountV2> GetAsync(int id)
        {
            var account = await _accounts.FirstOrDefaultAsync(x => x.Id == id);
            if (account == null)
            {
                throw new EntityNotFoundException($"account entity with id {id} was not found");
            }

            return account;
        }

        public async Task<List<AccountV2>> GetListAsync(string userId)
        {
            var query =
                from account in _accounts.Queryable
                join accountUser in _accountUsers.Queryable
                    on account.Id equals accountUser.AccountId
                where accountUser.UserId == userId
                select account;

            var accounts = await query.ToListAsync();
            return accounts;
        }

        public async Task<AccountV2> CreateAsync(
            string organizationName,
            string creatorId)
        {
            // add new account
            var account = new AccountV2(organizationName);
            await _accounts.AddAsync(account);

            // set current user as the account owner
            await CreateUserAsync(account.Id, creatorId, AccountUserRoleEnum.Owner.ToString());

            return account;
        }

        public async Task<AccountV2> UpdateAsync(AccountV2 updatedAccount)
        {
            return await _accounts.UpdateAsync(updatedAccount);
        }

        public async Task<bool> DeleteAsync(int accountId)
        {
            // delete account
            var removeAccount = await _accounts.DeleteAsync(accountId);

            // delete all account users
            var removeAccountUsers =
                await _accountUsers.DeleteAsync(accountUser => accountUser.AccountId == accountId);

            return removeAccount & removeAccountUsers;
        }

        public async Task<bool> IsOwnerAsync(int accountId, string userId)
        {
            var accountUser = await _accountUsers.FirstOrDefaultAsync(accountUser =>
                accountUser.AccountId == accountId &&
                accountUser.UserId == userId
            );

            return accountUser != null && accountUser.Role == AccountUserRoleEnum.Owner.ToString();
        }

        public async Task<bool> IsOwnerOrAdminAsync(int accountId, string userId)
        {
            var accountUser = await _accountUsers.FirstOrDefaultAsync(accountUser =>
                accountUser.AccountId == accountId &&
                accountUser.UserId == userId
            );

            var ownerAndAdmin = new[] { AccountUserRoleEnum.Owner.ToString(), AccountUserRoleEnum.Admin.ToString() };
            return accountUser != null && ownerAndAdmin.Contains(accountUser.Role);
        }

        public async Task<List<AccountUserV2>> GetUsersAsync(int accountId)
        {
            var accountUsers = await _accountUsers.Queryable.Where(x => x.AccountId == accountId).ToListAsync();
            return accountUsers;
        }

        public async Task<AccountUserV2> CreateUserAsync(
            int accountId,
            string userId,
            string role,
            string invitorUserId = null,
            string initialPassword = null)
        {
            var accountUser = new AccountUserV2(accountId, userId, role, invitorUserId, initialPassword);

            var created = await _accountUsers.AddAsync(accountUser);
            return created;
        }

        public async Task<bool> DeleteUserAsync(int accountId, string userId)
        {
            var deleteSuccess = await _accountUsers.DeleteAsync(accountUser =>
                accountUser.AccountId == accountId &&
                accountUser.UserId == userId
            );

            return deleteSuccess;
        }
    }
}