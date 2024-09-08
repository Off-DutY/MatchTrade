using StackExchange.Redis;

namespace CoreLibrary
{
    public static class RedisHelper
    {
        /// <summary>
        /// 刪除KEY資料
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        public static void DeleteKey(IDatabase db, string key)
        {
            if (db == null)
                return;
            db.KeyDeleteAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 回傳True 代表有KEY
        /// 回傳False 代表無KEY' KEY無資料' 不使用Redis
        /// </summary>
        /// <param name="db"></param>
        /// <param name="_type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool KeyExistsInCache(IDatabase db, RedisType _type, string key)
        {
            bool result = false;

            if (db == null)
                return result;

            switch (_type)
            {
                case RedisType.String:
                    result = db.KeyExistsAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
                    break;

                case RedisType.SortedSet:
                    result = db.SortedSetLengthAsync(key).ConfigureAwait(false).GetAwaiter().GetResult() > 0;
                    break;

                case RedisType.Set:
                    result = db.SetLengthAsync(key).ConfigureAwait(false).GetAwaiter().GetResult() > 0;
                    break;

                case RedisType.Hash:
                    result = db.HashLengthAsync(key).ConfigureAwait(false).GetAwaiter().GetResult() > 0;
                    break;

                case RedisType.List:
                    result = db.SortedSetLengthAsync(key).ConfigureAwait(false).GetAwaiter().GetResult() > 0;
                    break;
            }

            return result;
        }

        /// <summary>
        /// 新增string key 的值
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="_expire"></param>
        public static void SetStringValue(IDatabase db, string key, string value, TimeSpan? _expire)
        {
            if (db == null)
                return;

            if (_expire != null)
            {
                db.StringSetAsync(key, value, _expire.Value).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            else
            {
                db.StringSetAsync(key, value).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public static void SetStringIncrement(IDatabase db, string key, long value, TimeSpan? expire)
        {
            if (db == null)
            {
                return;
            }

            db.StringIncrementAsync(key, value, CommandFlags.None).ConfigureAwait(false).GetAwaiter().GetResult();
            if (expire.HasValue)
            {
                db.KeyExpireAsync(key, expire.Value).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 取得string key的值
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetString(IDatabase db, string key)
        {
            if (db == null)
                return string.Empty;

            if (!KeyExistsInCache(db, RedisType.String, key))
                return string.Empty;

            return db.StringGetAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static void SetAddValue<T>(IDatabase db, string key, T value, TimeSpan? expire)
        {
            if (db == null)
            {
                return;
            }

            db.SetAddAsync(key, value.ToString()).ConfigureAwait(false).GetAwaiter().GetResult();
            if (expire != null)
                db.KeyExpireAsync(key, expire.Value).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static List<T> SetGetAll<T>(IDatabase db, string key)
        {
            if (db == null)
            {
                return null;
            }

            if (KeyExistsInCache(db, RedisType.Set, key) == false)
            {
                return new List<T>();
            }

            var redisValues = db.SetMembersAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
            var cacheObj = redisValues.Cast<T>().ToList();
            return cacheObj;
        }

        /// <summary>
        /// 取得key過期時間
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TimeSpan? GetTimeToLive(IDatabase db, string key)
        {
            if (db is null)
                return TimeSpan.Zero;

            if (KeyExistsInCache(db, RedisType.String, key) == false)
                return TimeSpan.Zero;

            return db.KeyTimeToLiveAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
        }


        public static TimeSpan? Ping(IDatabase db)
        {
            var timeSpan = db?.Ping();
            return timeSpan;
        }

        public static bool LockTake(IDatabase db, string lockKey, long redisValue, TimeSpan expireTime)
        {
            if (db is null)
            {
                return false;
            }

            return db.LockTakeAsync(lockKey, redisValue, expireTime).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static void ReleaseLock(IDatabase db, string lockId, long lockValue)
        {
            if (db is null)
            {
                return;
            }

            db.LockReleaseAsync(lockId, lockValue).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}