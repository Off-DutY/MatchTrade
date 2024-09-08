using StackExchange.Redis;

namespace CoreLibrary.Interface
{
    public interface IRedisService
    {
        bool IsRedisEnable { get; }
        IDatabase NonePrefixDb { get; }
        IDatabase LockDb { get; }
    }
}