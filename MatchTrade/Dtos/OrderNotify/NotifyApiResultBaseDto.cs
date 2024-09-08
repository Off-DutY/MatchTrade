using System;
using System.Collections.Generic;

namespace MatchTrade.Dtos.OrderNotify
{
    public class NotifyApiResultBaseDto<T>
    {
        public NotifyApiResultBaseDto()
        {
        }

        public int Code { get; set; }

        public T Result { get; set; }

        public DateTime ResponseTime { get; set; }

        public List<NotifyApiErrorResultDto> ExtraInfo { get; set; }
    }
}