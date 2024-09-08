using CoreLibrary.Dto;
using CoreLibrary.Dto.BaseResponses;

namespace CoreLibrary.Interface
{
    public interface IApiResultService
    {
        ApiResultDto<T> Ok<T>(T structObj);
        ApiResultDto<T> Error<T>(Exception exception);
        ApiResultDto<T> CreateResult<T>(ApiResponseStatusCode statusCode, T obj);
        ApiResultDto<T> Match<T>(Result<T> result);
    }
}