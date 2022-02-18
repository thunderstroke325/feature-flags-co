using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using FeatureFlags.Utils.ConventionalDependencyInjection;

namespace FeatureFlags.APIs.Services
{
    public class FeatureFlagV2AppService : ITransientDependency
    {
        private readonly FeatureFlagV2Service _flagService;
        private readonly FeatureFlagTagTreeService _tagTreeService;

        public FeatureFlagV2AppService(
            FeatureFlagV2Service flagService,
            FeatureFlagTagTreeService tagTreeService)
        {
            _flagService = flagService;
            _tagTreeService = tagTreeService;
        }

        public async Task<PagedResult<FeatureFlagListViewModel>> GetListAsync(
            int envId,
            SearchFeatureFlagRequest request)
        {
            List<string> flagIds = null;

            // filter flags by tagIds
            var tagTrees = await _tagTreeService.FindAsync(envId);
            if (tagTrees != null &&
                request.TagIds != null &&
                request.TagIds.Any())
            {
                flagIds = new List<string>();
                foreach (var tagId in request.TagIds)
                {
                    var node = tagTrees.Node(tagId);
                    if (node != null)
                    {
                        var ids = node.Values().SelectMany(x => x);

                        flagIds.AddRange(ids);
                    }
                }

                flagIds = flagIds.Distinct().ToList();
            }

            var pagedFlags = await _flagService.GetListAsync(
                envId, request.Name, request.Status, flagIds, request.IncludeArchived, 
                request.PageIndex, request.PageSize
            );

            var vms = new List<FeatureFlagListViewModel>();
            foreach (var flag in pagedFlags.Items)
            {
                var tags = tagTrees == null
                    ? new List<string>()
                    : tagTrees.FlagTags(flag.Id);

                var vm = new FeatureFlagListViewModel
                {
                    Id = flag.Id,
                    Name = flag.Name,
                    Tags = tags,
                    Status = flag.Status,
                    LastModificationTime = flag.LastUpdatedTime,
                };
                vms.Add(vm);
            }

            return new PagedResult<FeatureFlagListViewModel>(pagedFlags.TotalCount, vms);
        }
    }
}