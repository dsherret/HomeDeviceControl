using System.Threading.Tasks;

namespace HomeDeviceControl.Communication.Common.StatusCheckers
{
    public interface IPowerStatusChecker
    {
        Task<bool> GetIsPoweredOnAsync();
    }
}
