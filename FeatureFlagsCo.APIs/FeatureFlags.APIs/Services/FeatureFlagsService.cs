using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using FeatureFlags.APIs.ViewModels.Environment;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Repositories
{
    public interface IFeatureFlagsService
    {
        Task<EnvironmentUserQueryResultViewModel> QueryEnvironmentFeatureFlagUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize, string currentUserId);
        Task<List<int>> GetAccountAndProjectIdByEnvironmentIdAsync(int environmentId);
    }

    public class FeatureFlagsService : IFeatureFlagsService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IGenericRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDistributedCache _redisCache;
        private readonly INoSqlService _cosmosDbService;

        public FeatureFlagsService(
            ApplicationDbContext context,
            IGenericRepository repository,
            UserManager<ApplicationUser> userManager,
            IDistributedCache redisCache,
            INoSqlService cosmosDbService)
        {
            _dbContext = context;
            _repository = repository;
            _userManager = userManager;
            _redisCache = redisCache;
            _cosmosDbService = cosmosDbService;
        }


        public async Task<EnvironmentUserQueryResultViewModel> QueryEnvironmentFeatureFlagUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize, string currentUserId)
        {
            var users = await _cosmosDbService.QueryEnvironmentUsersAsync(searchText, environmentId, pageIndex, pageSize);
            int pageTotalNumber = await _cosmosDbService.QueryEnvironmentUsersCountAsync(searchText, environmentId, pageIndex, pageSize);
            return new EnvironmentUserQueryResultViewModel
            {
                Count = pageTotalNumber,
                Users = users
            };
        }

        public async Task<List<int>> GetAccountAndProjectIdByEnvironmentIdAsync(int environmentId)
        {
            var returnList = new List<int>();
            var env = await _dbContext.Environments.FirstOrDefaultAsync(p => p.Id == environmentId);
            var proj = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == env.ProjectId);
            returnList.Add(proj.Id);
            returnList.Add(proj.AccountId);
            return returnList;
        }
    }

}
