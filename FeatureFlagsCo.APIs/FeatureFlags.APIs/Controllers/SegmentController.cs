using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.APIs.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/envs/{envId:int}/segment")]
    public class SegmentController : ApiControllerBase
    {
        private readonly SegmentService _service;
        private readonly SegmentAppService _appService;
        private readonly IMapper _mapper;

        public SegmentController(
            SegmentService service,
            SegmentAppService appService,
            IMapper mapper)
        {
            _service = service;
            _appService = appService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<SegmentListItem>> GetListAsync(int envId)
        {
            var segments = await _service.GetListAsync(envId);

            var vm = _mapper.Map<IEnumerable<Segment>, IEnumerable<SegmentListItem>>(segments);
            return vm;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<SegmentVm> GetAsync(string id)
        {
            var segment = await _service.GetAsync(id);

            var vm = _mapper.Map<Segment, SegmentVm>(segment);
            return vm;
        }

        [HttpPost]
        public async Task<SegmentVm> CreateAsync(int envId, UpsertSegment request)
        {
            var segment = new Segment(envId, request.Name, request.Included, request.Excluded, request.Description);

            var created = await _service.CreateAsync(segment);
            
            var vm = _mapper.Map<Segment, SegmentVm>(created);
            return vm;
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<SegmentVm> UpdateAsync(string id, UpsertSegment request)
        {
            var segment = await _service.GetAsync(id);
            segment.Update(request.Name, request.Included, request.Excluded, request.Description);

            var updated = await _service.UpdateAsync(segment);
            
            var vm = _mapper.Map<Segment, SegmentVm>(updated);
            return vm;
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<bool> DeleteAsync(int envId, string id)
        {
            var references = await _appService.GetFlagSegmentReferencesAsync(envId, id);
            if (references.Any())
            {
                throw new InvalidOperationException("cannot delete segment with existing flag references");
            }

            var isDeleted = await _service.DeleteAsync(id);
            return isDeleted;
        }

        [HttpGet]
        [Route("{id}/flag-segment-references")]
        public async Task<IEnumerable<FlagSegmentReference>> GetFlagSegmentReferencesAsync(int envId, string id)
        {
            var references = await _appService.GetFlagSegmentReferencesAsync(envId, id);
            return references;
        }
    }
}