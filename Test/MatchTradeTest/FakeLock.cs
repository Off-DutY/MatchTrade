using System.Collections.Generic;
using CoreLibrary.Interface;

namespace MatchTradeTest
{
    public class FakeLock : ILock
    {
        private static Dictionary<string, byte> _tempList = new Dictionary<string, byte>();

        public FakeLock(string lockId)
        {
            LockId = lockId;
            if (_tempList.ContainsKey(LockId))
            {
                IsAcquired = false;
                return;
            }

            lock (_tempList)
            {
                if (_tempList.ContainsKey(LockId))
                {
                    IsAcquired = false;
                    return;
                }

                _tempList.Add(LockId, 1);
                IsAcquired = true;
            }
        }

        public void Dispose()
        {
            if (IsAcquired)
                _tempList.Remove(LockId);
        }

        public string LockId { get; }
        public bool IsAcquired { get; }
    }
}