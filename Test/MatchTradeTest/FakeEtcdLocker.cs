using System.Threading.Tasks;
using CoreLibrary.Dto;
using CoreLibrary.Interface;
using Microsoft.Extensions.Logging;

namespace MatchTradeTest
{
    public class FakeEtcdLocker : IEtcdLockService
    {
        private async Task<ILock> Lock(ILogger logger, string lockKey)
        {
            var fakeLock = new FakeLock(lockKey);
            return await Task.FromResult(fakeLock);
        }

        public async Task<ILock> Lock(ILogger logger, string lockType, int memberId, string namespaceCode)
        {
            return await Lock(logger, $"{lockType}{memberId}{namespaceCode}");
        }

        public async Task<ILock> Lock(ILogger logger, string lockType, string uid, string namespaceCode)
        {
            return await Lock(logger, $"{lockType}{uid}{namespaceCode}");
        }

        public async Task<ILock> Lock(ILogger logger, string lockType, int memberId, string namespaceCode, EtcdLockOptionDto etcdLockOptionDto)
        {
            return await Lock(logger, $"{lockType}{memberId}{namespaceCode}");
        }

        public async Task<ILock> Lock(ILogger logger, string lockType, string uid, string namespaceCode, EtcdLockOptionDto etcdLockOptionDto)
        {
            return await Lock(logger, $"{lockType}{uid}{namespaceCode}");
        }
    }
}