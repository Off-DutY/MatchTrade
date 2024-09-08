using System;
using Data.MatchTrade.Models;

namespace MatchTrade.Dtos.Messages
{
    /// <summary>
    /// 撮合訂單DTO
    /// </summary>
    public class MatchOrderDto
    {
        public MatchOrderDto()
        {
        }

        public MatchOrderDto(OrderPool orderPool)
        {
            this.Id = orderPool.Id;
            this.SourceOrderId = orderPool.SourceOrderId;
            this.UserId = orderPool.UserId;
            this.Price = orderPool.Price;
            this.Number = orderPool.Number;
            this.Amount = orderPool.Amount;
            this.IsBuyOrder = orderPool.IsBuyOrder;
            this.OrderType = orderPool.OrderType;
            this.SymbolId = orderPool.SymbolId;
            this.State = orderPool.State;
            this.DealNum = orderPool.DealNum;
            this.NoDealNum = orderPool.NoDealNum;
            this.DealAmount = orderPool.DealAmount;
            this.NoDealAmount = orderPool.NoDealAmount;
            this.CreateTime = orderPool.CreateTime;
            this.UpdateTime = orderPool.UpdateTime;
            this.Priority = orderPool.Priority;
            this.MinPriceEachMatch = orderPool.MinPriceEachMatch;
            this.SourceSiteId = orderPool.SourceSiteId;
        }

        /// <summary>
        /// Identity
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 交易Id
        /// </summary>
        public string SourceOrderId { get; set; }

        /// <summary>
        /// 用戶Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 單價
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 數量
        /// </summary>
        public decimal Number { get; set; }

        /// <summary>
        /// 總額
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 是否為買單
        /// </summary>
        public bool IsBuyOrder { get; set; }

        /// <summary>
        /// 訂單類型
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// 交易對列Id
        /// </summary>
        public int SymbolId { get; set; }

        /// <summary>
        /// 狀態 - 參考 EnumOrderState
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 成交數量
        /// </summary>
        public decimal DealNum { get; set; }

        /// <summary>
        /// 未成交數量
        /// </summary>
        public decimal NoDealNum { get; set; }

        /// <summary>
        /// 成交總額
        /// </summary>
        public decimal DealAmount { get; set; }

        /// <summary>
        /// 未成交總額
        /// </summary>
        public decimal NoDealAmount { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 優先度
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 來源站台
        /// </summary>
        public int SourceSiteId { get; set; }

        /// <summary>
        /// 拆單的最小成交額度
        /// </summary>
        public decimal MinPriceEachMatch { get; set; }
    }
}