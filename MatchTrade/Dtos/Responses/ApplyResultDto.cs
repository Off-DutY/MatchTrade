using System;
using MatchTrade.Dtos.Messages;

namespace MatchTrade.Dtos.Responses
{
    public class ApplyResultDto
    {
        public ApplyResultDto(MatchOrderDto matchOrderDto)
        {
            this.Id = matchOrderDto.Id;
            this.SourceOrderId = matchOrderDto.SourceOrderId;
            this.UserId = matchOrderDto.UserId;
            this.Amount = matchOrderDto.Amount;
            this.IsBuyOrder = matchOrderDto.IsBuyOrder;
            this.OrderType = matchOrderDto.OrderType;
            this.SymbolId = matchOrderDto.SymbolId;
            this.State = matchOrderDto.State;
            this.DealAmount = matchOrderDto.DealAmount;
            this.NoDealAmount = matchOrderDto.NoDealAmount;
            this.CreateTime = matchOrderDto.CreateTime;
            this.UpdateTime = matchOrderDto.UpdateTime;
            this.SourceSiteId = matchOrderDto.SourceSiteId;
            this.MinPriceEachMatch = matchOrderDto.MinPriceEachMatch;
        }

        /// <summary>
        /// Identity
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 來源站台
        /// </summary>
        public int SourceSiteId { get; set; }

        /// <summary>
        /// 用戶Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 交易Id
        /// </summary>
        public string SourceOrderId { get; set; }

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
        /// 成交總額
        /// </summary>
        public decimal DealAmount { get; set; }

        /// <summary>
        /// 未成交總額
        /// </summary>
        public decimal NoDealAmount { get; set; }

        /// <summary>
        ///  最小交易金額
        /// </summary>
        public decimal MinPriceEachMatch { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}