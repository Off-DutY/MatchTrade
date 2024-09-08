namespace CoreLibrary.Dto
{
    public sealed class GetLockerFailedException : Exception
    {
        public GetLockerFailedException() : base("Failed to Get Lock")
        {
        }

        public GetLockerFailedException(string message) : base(message)
        {
        }

        public GetLockerFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}