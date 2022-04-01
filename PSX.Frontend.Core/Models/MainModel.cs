using PSX.Frontend.Core.Services;

namespace PSX.Frontend.Core.Models;

public sealed class MainModel
{
    public MainModel(IFileDialogService fileDialogService, IShutdownService shutdownService)
    {
        FileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        ShutdownService   = shutdownService ?? throw new ArgumentNullException(nameof(shutdownService));
    }

    private IFileDialogService FileDialogService { get; }

    private IShutdownService ShutdownService { get; }

    public void OpenFile()
    {
        const string filter = "Everything|*.exe;*.psx;*.bin;*.cue|Application|*.exe;*.psx|Image|*.bin;*.cue";

        var path = FileDialogService.OpenFile(filter);

        if (path is null)
            return;

        path.ToString();
    }

    public void Shutdown()
    {
        ShutdownService.Shutdown();
    }
}