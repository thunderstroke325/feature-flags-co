using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Account;
using FeatureFlags.APIs.ViewModels.Project;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Repositories
{
    public interface IAccountService
    {
        Task<List<AccountViewModel>> GetAccountsAsync(string userId);
        Task<AccountViewModel> CreateAccountAsync(string currentUserId, AccountViewModel param, bool isInitializingAccount = false);
        Task DeleteAccountAsync(int accountId);
    }

    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IGenericRepository _repository;
        private readonly IAccountUserService _accountUserService;
        private readonly IProjectService _projectService;

        public AccountService(ApplicationDbContext context, IGenericRepository repository,
            IAccountUserService accountUserService,
            IProjectService projectService)
        {
            _dbContext = context;
            _repository = repository;
            _accountUserService = accountUserService;
            _projectService = projectService;
        }

        public async Task<List<AccountViewModel>> GetAccountsAsync(string userId)
        {
            var query = from aum in _dbContext.AccountUserMappings
                        join account in _dbContext.Accounts
                        on aum.AccountId equals account.Id
                        where aum.UserId == userId
                        select new AccountViewModel
                        {
                            Id = account.Id,
                            OrganizationName = account.OrganizationName
                        };
            return await query.ToListAsync();
        }

        public async Task<AccountViewModel> CreateAccountAsync(string currentUserId, AccountViewModel param, bool isInitializingAccount = false)
        {
            var now = DateTime.UtcNow;
            var newAccount = await _repository.CreateAsync<Account>(new Account
            {
                OrganizationName = param.OrganizationName,
                CreatedAt = now,
                UpdatedAt = now
            });
            
            // set current user as the account owner
            await _repository.CreateAsync<AccountUserMapping>(new AccountUserMapping
            {
                Role = AccountUserRoleEnum.Owner.ToString(),
                UserId = currentUserId,
                AccountId = newAccount.Id
            });

            // create default project
            await this._projectService.CreateProjectAsync(currentUserId, newAccount.Id, new ProjectViewModel { Name = "Default Project" }, isInitializingAccount);

            return new AccountViewModel
            {
                Id = newAccount.Id,
                OrganizationName = newAccount.OrganizationName
            };
        }

        public async Task DeleteAccountAsync(int accountId)
        {
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(p => p.Id == accountId);

            // Remove project
            await _projectService.RemoveAllProjectsAsync(account.Id);

            // Remove account users
            await _accountUserService.RemoveAllUsersAsync(accountId);

            // Remove account
            _dbContext.Accounts.Remove(account);

            await _dbContext.SaveChangesAsync();
        }
    }
}
