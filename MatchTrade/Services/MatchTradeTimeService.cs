using System;
using CoreLibrary.Interface;
using Microsoft.Extensions.Configuration;

namespace MatchTrade.Services
{
    public class MatchTradeTimeService : ITimeService
    {
        private readonly IConfiguration _configuration;
        private string _systemTimeZone;

        public MatchTradeTimeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region 時間類
        
        /// <summary>
        /// 取得现在时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetNow()
        {
            //取得當前系統時區
            var systemTimeZone = SystemTimeZone;

            return new DateTime(DateTime.UtcNow.AddHours(Convert.ToDouble(systemTimeZone)).Ticks);
        }

        #endregion 時間類
        
        /// <summary>
        /// 從 appSettings 讀出【SystemTimeZone】
        /// </summary>
        public string SystemTimeZone
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_systemTimeZone))
                {
                    _systemTimeZone = _configuration.GetSection(nameof(SystemTimeZone)).Value;

                    if (string.IsNullOrWhiteSpace(_systemTimeZone))
                    {
                        throw new Exception($"無法從 appsettings 取得 {nameof(SystemTimeZone)}");
                    }
                }

                return _systemTimeZone;
            }
            set => _systemTimeZone = value;
        }

    }
}