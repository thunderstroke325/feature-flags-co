using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.Utils.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/accounts")]
    public class AccountsV2Controller : ApiControllerBase
    {
        private readonly AccountV2Service _accountService;
        private readonly AccountV2AppService _accountAppService;
        private readonly IMapper _objectMapper;

        public AccountsV2Controller(
            AccountV2Service accountService,
            AccountV2AppService accountAppService,
            IMapper objectMapper)
        {
            _accountService = accountService;
            _objectMapper = objectMapper;
            _accountAppService = accountAppService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<AccountViewModel> GetAsync(int id)
        {
            var account = await _accountService.GetAsync(id);

            var vm = _objectMapper.Map<AccountV2, AccountViewModel>(account);
            return vm;
        }

        [HttpGet]
        public async Task<IEnumerable<AccountViewModel>> GetListAsync()
        {
            var accounts = await _accountService.GetListAsync(CurrentUserId);

            var vm = _objectMapper.Map<IEnumerable<AccountV2>, IEnumerable<AccountViewModel>>(accounts);
            return vm;
        }

        [HttpPost]
        public async Task<AccountViewModel> CreateAsync(AccountViewModel input)
        {
            var account = await _accountAppService.CreateAsync(input.OrganizationName, CurrentUserId);

            var vm = _objectMapper.Map<AccountV2, AccountViewModel>(account);
            return vm;
        }

        [HttpPut]
        public async Task<AccountV2> UpdateAsync(AccountViewModel input)
        {
            var account = await _accountService.GetAsync(input.Id);

            // only account owner can update an account
            var isAccountOwner = await _accountService.IsOwnerAsync(account.Id, CurrentUserId);
            if (!isAccountOwner)
            {
                throw new PermissionDeniedException(
                    "only account owner can update the account, you don't have permission");
            }

            account.Update(input.OrganizationName);

            var updated = await _accountService.UpdateAsync(account);
            return updated;
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<bool> DeleteAsync(int id)
        {
            var account = await _accountService.GetAsync(id);

            // only account owner can delete an account
            var isAccountOwner = await _accountService.IsOwnerAsync(account.Id, CurrentUserId);
            if (!isAccountOwner)
            {
                throw new PermissionDeniedException(
                    "only account owner can delete the account, you don't have permission");
            }

            var success = await _accountAppService.DeleteAsync(id);
            return success;
        }
    }
}