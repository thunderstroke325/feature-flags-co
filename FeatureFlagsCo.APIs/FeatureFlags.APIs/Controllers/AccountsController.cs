using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers
{
    [ApiVersion("1.0")]
    public class AccountsController : ApiControllerBase
    {
        private readonly IGenericRepository _repository;
        private readonly IAccountService _accountService;
        private readonly IAccountUserService _accountUserService;

        public AccountsController(
            IGenericRepository repository,
            IAccountService accountService,
            IAccountUserService accountUserService)
        {
            _repository = repository;
            _accountService = accountService;
            _accountUserService = accountUserService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<AccountViewModel> GetAccount(int id)
        {
            var account = await _repository.SelectByIdAsync<Account>(id);

            return new AccountViewModel
            {
                Id = account.Id,
                OrganizationName = account.OrganizationName
            };
        }

        [HttpGet]
        [Route("")]
        public async Task<List<AccountViewModel>> GetAccounts()
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
           return await _accountService.GetAccountsAsync(currentUserId);
        }


        [HttpPost]
        [Route("")]
        public async Task<dynamic> CreateAccount([FromBody]AccountViewModel param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            return await _accountService.CreateAccountAsync(currentUserId, param);
        }

        [HttpPut]
        [Route("")]
        public async Task<dynamic> UpdateAccount([FromBody]AccountViewModel param)
        {
            var account = await _repository.SelectByIdAsync<Account>(param.Id);

            if (account != null)
            {
                // Only account owner can update an account
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                var accountOwner = await _accountUserService.GetAccountOwnerAsync(account.Id);

                if (accountOwner == null || currentUserId != accountOwner.UserId) 
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
                }

                account.OrganizationName = param.OrganizationName;
                account.UpdatedAt = DateTime.UtcNow;
                return await _repository.UpdateAsync<Account>(account);
            }

            return StatusCode(StatusCodes.Status404NotFound);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<dynamic> DeleteAccount(int id)
        {
            var account = await _repository.SelectByIdAsync<Account>(id);

            if (account != null)
            {
                // Only account owner can update an account
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
                var accontOwner = await _accountUserService.GetAccountOwnerAsync(account.Id);

                if (currentUserId != accontOwner.UserId)
                {
                    return StatusCode (StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
                }

                await _accountService.DeleteAccountAsync(account.Id);
            }

            return Ok();
        }
    }
}
