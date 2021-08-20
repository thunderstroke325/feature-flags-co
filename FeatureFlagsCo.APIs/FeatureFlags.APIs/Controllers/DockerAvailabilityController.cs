using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [ApiController]
    [Route("[controller]")]
    public class DockerAvailabilityController : ControllerBase
    {
        private readonly IOptions<MySettings> _mySettings;
        private readonly INoSqlService _noSqlService;
        public DockerAvailabilityController(IOptions<MySettings> mySettings,
            INoSqlService noSqlService)
        {
            _mySettings = mySettings;
            _noSqlService = noSqlService;
        }

        [HttpGet]
        [Route("GetHostType")]
        public string GetHostingType()
        {
            return _mySettings.Value.HostingType + " " + _noSqlService.GetType().FullName + " " + _mySettings.Value.InsightsRabbitMqUrl;
        }
    }
}
