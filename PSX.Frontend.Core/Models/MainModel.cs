using PSX.Frontend.Core.Services;

namespace PSX.Frontend.Core.Models;

public sealed class MainModel
{
    public MainModel(IFileDialogService fileDialogService, IApplicationService applicationService)
    {
        FileDialogService  = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        ApplicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    private IFileDialogService FileDialogService { get; }

    private IApplicationService ApplicationService { get; }

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
        ApplicationService.Shutdown();
    }
}