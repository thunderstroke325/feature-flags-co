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
using FeatureFlags.APIs.ViewModels.Analytic;
using FeatureFlagsCo.MQ.ElasticSearch;

namespace FeatureFlags.APIs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticBoardsController : ControllerBase
    {
        private readonly MongoDbAnalyticBoardService _mongoDbAnalyticBoardService;
        private readonly ElasticSearchService _elasticSearchService;
        private readonly IEnvironmentService _envService;

        public AnalyticBoardsController(
            MongoDbAnalyticBoardService mongoDbAnalyticBoardService,
            ElasticSearchService elasticSearchService, 
            IEnvironmentService envService)
        {
            _mongoDbAnalyticBoardService = mongoDbAnalyticBoardService;
            _envService = envService;
            _elasticSearchService = elasticSearchService;
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
                        Dimensions = new List<AnalyticDimension>(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    board = await _mongoDbAnalyticBoardService.CreateAsync(data);
                }

                return board;
            }

            return StatusCode(StatusCodes.Status401Unauthorized, new Response { Code = "Error", Message = "Unauthorized" });
        }

        [HttpPost]
        [Route("results")]
        public async Task<dynamic> CalculateResults([FromBody] CalculationParam param)
        {
            var currentUserId = HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (!await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }
            
            // if no item, return empty list
            if (!param.Items.Any())
            {
                return new List<CalculationItemResultViewModel>();
            }

            var searchResult = await _elasticSearchService.SearchDocumentAsync(param.SearchAggregationDescriptor());
            var itemResults = param.Items.Select(item =>
            {
                var aggregationValues = searchResult.Aggregations.Children(item.DataSource.KeyName);
                return new CalculationItemResultViewModel
                {
                    Id = item.Id,
                    Value = item.AggregationValue(aggregationValues).GetValueOrDefault()
                };
            });

            return new CalculationResultsViewModel { Items = itemResults };
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
            
            board.UpsertDataSource(param.Id, param.Name, param.KeyName, param.DataType);
            
            var updatedBoard = await _mongoDbAnalyticBoardService.UpdateAsync(board.Id, board);
            return updatedBoard.DataSourceDefs.FirstOrDefault(x => x.Id == param.Id);
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
                
            var updatedBoard = await _mongoDbAnalyticBoardService.UpdateAsync(board.Id, board);
            return updatedBoard.DataGroups.FirstOrDefault(x => x.Id == param.Id);
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
        
        [HttpPost("dimension")]
        public async Task<dynamic> UpsertAnalyticDimension(DataDimensionViewModel param)
        {
            var currentUserId = User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (!await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, param.EnvId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }
            
            var board = await _mongoDbAnalyticBoardService.GetAsync(param.AnalyticBoardId);
            if (board == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Code = "Error", Message = "The board does not exist." });
            }
            
            board.UpsertAnalyticDimension(param.Id, param.Key, param.Value);
            
            var updatedBoard = await _mongoDbAnalyticBoardService.UpdateAsync(board.Id, board);
            return updatedBoard.Dimensions.FirstOrDefault(x => x.Id == param.Id);
        }

        [HttpDelete("dimension")]
        public async Task<dynamic> DeleteAnalyticDimension(int envId, string boardId, string dimensionId)
        {
            var currentUserId = User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (!await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }
            
            await _mongoDbAnalyticBoardService.RemoveAnalyticDimensionAsync(boardId, dimensionId);
            
            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
