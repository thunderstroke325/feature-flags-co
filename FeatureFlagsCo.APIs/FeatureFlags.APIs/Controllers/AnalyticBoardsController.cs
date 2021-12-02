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

        [HttpPost]
        [Route("results")]
        public async Task<dynamic> CalculateResults([FromBody] CalculationParam param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId))
            {
                IEnumerable<CalculationItemResultViewModel> itemResults = null;
                if (param.Items.Count == 0)
                {
                    itemResults = new List<CalculationItemResultViewModel>();
                }
                else 
                {
                    // TODO calculate values
                    Random rd = new Random();
                    itemResults = param.Items.Select(i => new CalculationItemResultViewModel { Id = i.Id, Value = (double)rd.Next(1, 1000) });
                }

                return new CalculationResultsViewModel { Items = itemResults };
            }

            return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
        }

        [HttpPost]
        [Route("data-source")]
        public async Task<dynamic> UpsertDataSource([FromBody] DataSourceDefViewModel param)
        {
            var currentUserId = HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (!await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }
            
            var board = await _mongoDbAnalyticBoardService.GetAsync(param.AnalyticBoardId);
            if (board == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Code = "Error", Message = "The board does not exist." });
            }
            
            board.UpsertDataSource(param.Id, param.Name, param.DataType);
            
            var updatedBoard = await _mongoDbAnalyticBoardService.UpdateAsync(board.Id, board);
            return updatedBoard;
        }

        [HttpDelete("data-source")]
        public async Task<dynamic> DeleteDateSource(int envId, string boardId, string dataSourceId)
        {
            var currentUserId = HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (!await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            await _mongoDbAnalyticBoardService.RemoveDataSourceAsync(boardId, dataSourceId);
            
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost]
        [Route("data-group")]
        public async Task<dynamic> UpsertDataGroup([FromBody] DataGroupViewModel param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (!await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }
            
            var board = await _mongoDbAnalyticBoardService.GetAsync(param.AnalyticBoardId);
            if (board == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Code = "Error", Message = "Bad Request" });
            }

            board.UpsertDataGroup(param.Id, param.Name, param.StartTime, param.EndTime, param.Items);
                
            var result = await _mongoDbAnalyticBoardService.UpdateAsync(board.Id, board);
            return result.DataGroups.FirstOrDefault(x => x.Id == param.Id);
        }

        [HttpDelete]
        [Route("data-group")]
        public async Task<dynamic> DeleteDataGroup(int envId, string boardId, string groupId)
        {
            var currentUserId = HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (!await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            await _mongoDbAnalyticBoardService.RemoveDataGroupAsync(boardId, groupId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
