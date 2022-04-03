using PSX.Frontend.Core.Services;

namespace PSX.Frontend.Core.Models;

public sealed class MainModel
{
    public MainModel(IApplicationService applicationService, IFileService fileService)
    {
        ApplicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
        FileService        = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    private IApplicationService ApplicationService { get; }

    private IFileService FileService { get; }

    public void OpenFile()
    {
        const string filter = "Everything|*.exe;*.psx;*.bin;*.cue|Application|*.exe;*.psx|Image|*.bin;*.cue";

        var path = FileService.OpenFile(filter);

        if (path is null)
            return;

        path.ToString();
    }

    public void Shutdown()
    {
        ApplicationService.Shutdown();
    }
}