using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticBoardsController : ControllerBase
    {
        private readonly MongoDbAnalyticBoardService _mongoDbAnalyticBoardService;
        private readonly IEnvironmentService _envService;

        public AnalyticBoardsController(
            MongoDbAnalyticBoardService mongoDbAnalyticBoardService,
            IEnvironmentService envService)
        {
            _mongoDbAnalyticBoardService = mongoDbAnalyticBoardService;
            _envService = envService;
        }

        [HttpGet]
        [Route("{envId}")]
        public async Task<dynamic> Get(int envId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                var board = await _mongoDbAnalyticBoardService.GetByEnvIdAsync(envId);

                if (board == null) 
                {
                    var data = new AnalyticBoard
                    {
                        EnvId = envId,
                        DataSourceDefs = new List<DataSourceDef>(),
                        DataGroups = new List<DataGroup>(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    board = await _mongoDbAnalyticBoardService.CreateAsync(data);
                }

                return new AnalyticBoardViewModel
                {
                    Id = board.Id,
                    EnvId = board.EnvId,
                    DataSourceDefs = board.DataSourceDefs,
                    DataGroups = board.DataGroups.Select(d => new DataGroupViewModel
                    {
                        Id = d.Id,
                        Name = d.Name,
                        StartTime = d.StartTime,
                        EndTime = d.EndTime,
                        Items = d.Items
                    }).ToList()
                };
            }

            return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
        }

        //[HttpPost]
        //[Route("data-source")]
        //public async Task<dynamic> UpdateDataSourceDef([FromBody] DataSourceDefViewModel param)
        //{
        //    var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
        //    if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId))
        //    {
        //        var board = await _mongoDbAnalyticBoardService.GetAsync(param.AnalyticBoardId);
        //        if (board == null)
        //        {
        //            return StatusCode(StatusCodes.Status400BadRequest, new Response { Code = "Error", Message = "Bad Request" });
        //        }
        //        else
        //        {
        //            board.DataSourceDefs = param.DataSourceDefs;

        //            return await _mongoDbAnalyticBoardService.UpdateAsync(board.Id, board);
        //        }
        //    }

        //    return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
        //}

        //[HttpPost]
        //[Route("data-group")]
        //public async Task<dynamic> UpsertDataGroup([FromBody] DataGroupViewModel param)
        //{
        //    var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
        //    if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId))
        //    {
        //        var board = await _mongoDbAnalyticBoardService.GetAsync(param.AnalyticBoardId);
        //        if (board == null)
        //        {
        //            return StatusCode(StatusCodes.Status400BadRequest, new Response { Code = "Error", Message = "Bad Request" });
        //        }
        //        else
        //        {
        //            var dataGroup = board.DataGroups.FirstOrDefault(x => x.Id == param.Id);
        //            if (dataGroup == null) 
        //            {
        //                dataGroup = new DataGroup
        //                {
        //                    Id = param.Id
        //                };
        //            }

        //            dataGroup.Name = param.Name;
        //            dataGroup.StartTime = param.StartTime;
        //            dataGroup.EndTime = param.EndTime;
        //            dataGroup.Items = param.Items;

        //            return await _mongoDbAnalyticBoardService.UpdateAsync(board.Id, board);
        //        }
        //    }

        //    return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
        //}
    }
}
