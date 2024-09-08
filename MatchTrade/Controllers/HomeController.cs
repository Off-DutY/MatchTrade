using Data.MatchTrade.Logic;
using CoreLibrary;
using CoreLibrary.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MatchTrade.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAwakeDbContextLogic _awakeDbContextLogic;
        private readonly IRedisService _redisService;
        private readonly ITimeService _timeService;

        public HomeController(ILogger<HomeController> logger,
                              IAwakeDbContextLogic awakeDbContextLogic,
                              IRedisService redisService,
                              ITimeService timeService)
        {
            _logger = logger;
            _awakeDbContextLogic = awakeDbContextLogic;
            _redisService = redisService;
            _timeService = timeService;
        }

        [AllowAnonymous]
        [HttpGet(nameof(Awake))]
        public string Awake()
        {
            var now = _timeService.GetNow();
            _logger.LogDebug($"Current Time is {now.ToLongTimeString()}");
            _awakeDbContextLogic.Awake();
            return "ok";
        }
        
        [AllowAnonymous]
        [HttpGet(nameof(PingRedis))]
        public string PingRedis()
        {
            var timeSpan = RedisHelper.Ping(_redisService.NonePrefixDb);
            return timeSpan.ToString();
        }
    }
}