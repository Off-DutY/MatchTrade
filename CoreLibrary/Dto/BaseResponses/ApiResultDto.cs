using System.Net;
using Newtonsoft.Json;

namespace CoreLibrary.Dto.BaseResponses
{
    public sealed class ApiResultDto<T>
    {
        /// <summary>
        /// 呼叫API接口狀態
        /// </summary>
        public int Code { get; set; } = (int)HttpStatusCode.OK;

        /// <summary>
        /// 根據API接口決定各自定義結果
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// 錯誤訊息，當Code為400時才會有值
        /// 為空時，不會出現在Response
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ApiExtraMessageDto> ExtraInfo { get; set; }

        /// <summary>
        /// API回應時間
        /// </summary>
        public DateTime ResponseTime { get; set;}

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Ip { get; set; }

        public ApiResultDto<T> AddExtraInfo(string infoTitle, string infoMessage)
        {
            if (this.ExtraInfo is null)
            {
                this.ExtraInfo = new List<ApiExtraMessageDto>();
            }

            this.ExtraInfo.Add(new ApiExtraMessageDto(infoTitle, infoMessage));
            return this;
        }
    }
}