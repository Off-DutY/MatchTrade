namespace MatchTrade.Dtos.Messages
{
    public class NotifyRuleDto
    {
        /// <summary>
        /// 每次通知N個MM商
        /// </summary>
        public int TakerPoolMaxLimit { get; set; } = 1;

        /// <summary>
        /// 總發送次數
        /// </summary>
        public int SendTakingOrderTimes { get; set; } = 1;

        /// <summary>
        /// 發送接單需求之後，總共等待時間，包含WaitingTimeBeforeCheckResponseInMilliseconds的秒數
        /// </summary>
        public int TotalRespondWaitingTimeInMilliseconds { get; set; }

        /// <summary>
        /// 發送接單需求之後，到開始確認回報狀態之前的等待時間，因為不可能秒按秒回，所以沒必要發送完成之後馬上確認狀態
        /// </summary>
        public int WaitingTimeBeforeCheckResponseInMilliseconds { get; set; }

        /// <summary>
        /// 確認狀態的間隔時間
        /// </summary>
        public int IntervalTimeBetweenCheckRedisValueInMilliseconds { get; set; }
    }
}