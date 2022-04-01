using System.Diagnostics.CodeAnalysis;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using PSX.Frontend.Core.Services;

namespace PSX.Frontend.Core.Models;

internal sealed class MainModel
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    public void OpenFile()
    {
        const string filter = "Everything|*.exe;*.psx;*.bin;*.cue|Application|*.exe;*.psx|Image|*.bin;*.cue";

        var service = Ioc.Default.GetRequiredService<IFileDialogService>();

        var path = service.OpenFile(filter);

        if (path is null)
            return;

        path.ToString();
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    public void Shutdown()
    {
        var service = Ioc.Default.GetRequiredService<IShutdownService>();

        service.Shutdown();
    }
}