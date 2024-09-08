using CoreLibrary.Interface;
using StackExchange.Redis;

namespace MatchTradeTest
{
    public sealed class FakeRedisService : IRedisService
    {
        public bool IsRedisEnable { get; }
        public IDatabase NonePrefixDb { get; }
        public IDatabase LockDb { get; }
        public IDatabase DataCenterDb { get; }
        public IDatabase ConfigCenterDb { get; }
        public IDatabase ConnectionCenterDb { get; }
        public IDatabase StructuralDefinitionDb { get; }
        public IDatabase CasinoDB { get; }
        public IDatabase RheaStateDb { get; }
    }
}