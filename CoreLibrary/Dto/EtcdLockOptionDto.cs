namespace CoreLibrary.Dto
{
    public sealed class EtcdLockOptionDto
    {
        // Lock設定相關最大值
        private const int MaxLockTime = 3600;
        private const int MaxWaitConnectionTimeout = 30;
        private const int MaxLockRetryCount = 30;

        // Lock設定相關預設值
        private const int DefaultLockTime = 60;
        private const int DefaultWaitConnectionTimeout = 10;
        private const int DefaultLockRetryCount = 3;

        /// <summary>
        /// Lock時間設定
        /// </summary>
        /// <exception cref="Exception"></exception>
        public EtcdLockOptionDto()
        {
            LockTime = DefaultLockTime;
            WaitConnectionTimeout = DefaultWaitConnectionTimeout;
            MaxRetryCount = DefaultLockRetryCount;
        }

        /// <summary>
        /// Lock持有時間
        /// </summary>
        private int _lockTime;

        /// <summary>
        /// 等待取得Lock的時間
        /// </summary>
        private int _waitConnectionTimeout;

        /// <summary>
        /// 嘗試取得的次數
        /// </summary>
        private int _maxRetryCount;

        /// <summary>
        /// 預設持有Lock時間60秒, 最大3600秒
        /// </summary>
        public int LockTime
        {
            get => _lockTime;
            set
            {
                if (value > MaxLockTime)
                {
                    throw new Exception("LockTime設定超過最大值, 請重新確認");
                }

                _lockTime = value;
            }
        }

        /// <summary>
        /// 預設等待拿到Lock時間10秒, 最大30秒
        /// </summary>
        public int WaitConnectionTimeout
        {
            get => _waitConnectionTimeout;
            set
            {
                if (value > MaxWaitConnectionTimeout)
                {
                    throw new Exception("WaitConnectionTimeout設定超過最大值, 請重新確認");
                }

                _waitConnectionTimeout = value;
            }
        }

        /// <summary>
        /// 預設等待重試次數3次, 最大30次
        /// </summary>
        public int MaxRetryCount
        {
            get => _maxRetryCount;
            set
            {
                if (value > MaxLockRetryCount)
                {
                    throw new Exception("MaxRetryCount超過最大值, 請重新確認");
                }

                _maxRetryCount = value;
            }
        }
    }
}