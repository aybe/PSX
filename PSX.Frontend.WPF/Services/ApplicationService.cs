using System.Windows;
using PSX.Frontend.Core.Old.Services;

namespace PSX.Frontend.WPF.Services;

internal sealed class ApplicationService : IApplicationService
{
    void IApplicationService.Shutdown()
    {
        Application.Current.Shutdown();
    }
}