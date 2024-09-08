namespace Data.MatchTrade.Factories.Interfaces
{
    public interface IMatchTradeContextFactory
    {
        MatchTradeContext GetInstance { get; }
        MatchTradeContext GetInstanceReadOnly { get; }
        MatchTradeContext GetNewInstance(bool isReadOnly);
    }
}