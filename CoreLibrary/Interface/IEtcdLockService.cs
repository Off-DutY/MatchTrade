using CoreLibrary.Dto;
using Microsoft.Extensions.Logging;

namespace CoreLibrary.Interface
{
    public interface IEtcdLockService
    {
        Task<ILock> Lock(ILogger logger, string lockType,
                         int memberId, string namespaceCode);

        Task<ILock> Lock(ILogger logger, string lockType,
                         string uid, string namespaceCode);

        Task<ILock> Lock(ILogger logger, string lockType,
                         int memberId, string namespaceCode,
                         EtcdLockOptionDto etcdLockOptionDto);

        Task<ILock> Lock(ILogger logger, string lockType,
                         string uid, string namespaceCode,
                         EtcdLockOptionDto etcdLockOptionDto);
    }
}