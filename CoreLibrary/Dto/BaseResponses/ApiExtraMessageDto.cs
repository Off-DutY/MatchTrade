namespace CoreLibrary.Dto.BaseResponses
{
    public sealed class ApiExtraMessageDto
    {
        public ApiExtraMessageDto(string field = null, string value = null)
        {
            Field = field;
            Value = value ?? "伺服器繁忙，请稍后再试";
        }

        public string Field { get; set; }

        public string Value { get; set; }
    }
}