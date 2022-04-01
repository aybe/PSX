using System.Windows;
using PSX.Frontend.Core.Services;

namespace PSX.Frontend.Services;

internal sealed class ShutdownService : IShutdownService
{
    public void Shutdown()
    {
        Application.Current.Shutdown();
    }
}