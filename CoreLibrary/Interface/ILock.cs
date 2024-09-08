namespace CoreLibrary.Interface
{
    public interface ILock : IDisposable
    {
        string LockId { get; }

        bool IsAcquired { get; }
    }
}