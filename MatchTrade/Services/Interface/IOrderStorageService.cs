using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data.MatchTrade.Models;
using MatchTrade.Dtos.Messages;
using MatchTrade.Dtos.Responses;

namespace MatchTrade.Services.Interface
{
    public interface IOrderStorageService
    {
        Task<List<OrderPool>> GetMatchingPool();
        Task<Dictionary<long, MatchOrderDto>> GetOrderFromPool(string orderBookKey, MatchOrderDto matchOrderDto, CancellationToken cancellationToken);
        IEnumerable<long> GetOrderBookHead(Dictionary<long, MatchOrderDto> outOrderBook);
        Task<(bool isSuccess, MatchOrderDto taker)> AddToOrderBook(MatchOrderDto matchOrderDto, CancellationToken cancellationToken);
        Task<MatchOrderDto> UpdateOrderBook(MatchOrderDto matchOrderDto);
        Task<OrderPool> GetOrderFromPoolById(string orderBookKey, long makerId, bool readOnly);
        Task SaveChanges();
        Task AddTradeResults(List<TradeResultDto> matchResultTradeResults);
    }
}