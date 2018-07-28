using System.Threading.Tasks;

namespace LightControl.Communication.Common.StatusCheckers
{
    public interface IPowerStatusChecker
    {
        Task<bool> GetIsPoweredOnAsync();
    }
}
