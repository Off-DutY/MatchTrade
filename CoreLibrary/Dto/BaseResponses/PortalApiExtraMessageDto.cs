namespace CoreLibrary.Dto.BaseResponses
{
    public sealed class PortalApiExtraMessageDto
    {
        public PortalApiExtraMessageDto(string field = null, string value = null)
        {
            Field = field;
            Message = value ?? "伺服器繁忙，请稍后再试";
        }

        public string Field { get; set; }

        public string Message { get; set; }
    }
}