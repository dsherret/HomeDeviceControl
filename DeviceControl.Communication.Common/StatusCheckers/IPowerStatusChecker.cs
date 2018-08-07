using System.Threading.Tasks;

namespace DeviceControl.Communication.Common.StatusCheckers
{
    public interface IPowerStatusChecker
    {
        Task<bool> GetIsPoweredOnAsync();
    }
}
