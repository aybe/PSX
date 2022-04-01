using PSX.Frontend.Core.Services;

namespace PSX.Frontend.Core.Models;

public sealed class MainModel
{
    public MainModel(IStorageService storageService, IApplicationService applicationService)
    {
        StorageService     = storageService ?? throw new ArgumentNullException(nameof(storageService));
        ApplicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    private IStorageService StorageService { get; }

    private IApplicationService ApplicationService { get; }

    public void OpenFile()
    {
        const string filter = "Everything|*.exe;*.psx;*.bin;*.cue|Application|*.exe;*.psx|Image|*.bin;*.cue";

        var path = StorageService.OpenFile(filter);

        if (path is null)
            return;

        path.ToString();
    }

    public void Shutdown()
    {
        ApplicationService.Shutdown();
    }
}