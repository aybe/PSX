using Microsoft.Extensions.Options;
using PSX.Frontend.Services;
using PSX.Frontend.Services.Emulation;

namespace PSX.Frontend.Interface;

public sealed class MainModel
    : IEmulatorControlService // for view model commands
{
    public MainModel(
        IApplicationService applicationService,
        IEmulatorControlService emulatorControlService,
        IFileService fileService,
        IOptions<AppSettings> appSettings
    )
    {
        ApplicationService     = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
        EmulatorControlService = emulatorControlService ?? throw new ArgumentNullException(nameof(emulatorControlService));
        FileService            = fileService ?? throw new ArgumentNullException(nameof(fileService));
        AppSettings            = appSettings;
    }

    private IApplicationService ApplicationService { get; }

    private IEmulatorControlService EmulatorControlService { get; }

    private IFileService FileService { get; }

    private IOptions<AppSettings> AppSettings { get; }

    public void OpenFile()
    {
        const string filter = "Everything|*.exe;*.psx;*.bin;*.cue|Application|*.exe;*.psx|Image|*.bin;*.cue";

        var path = FileService.OpenFile(filter);

        if (path is null)
            return;

        var list = AppSettings.Value.RecentlyUsed;

        for (var i = list.Count - 1; i >= 0; i--)
        {
            if (string.Equals(list[i], path, StringComparison.OrdinalIgnoreCase))
            {
                list.RemoveAt(i);
            }
        }

        list.Insert(0, path);

        while (list.Count > 10)
        {
            list.RemoveAt(list.Count - 1);
        }

        EmulatorControlService.Setup(path);
    }

    public void Shutdown()
    {
        ApplicationService.Shutdown();
    }

    #region IEmulatorControlService

    public bool CanStart => EmulatorControlService.CanStart;

    public bool CanStop => EmulatorControlService.CanStop;

    public bool CanPause => EmulatorControlService.CanPause;

    public bool CanContinue => EmulatorControlService.CanContinue;

    public bool CanFrame => EmulatorControlService.CanFrame;

    public void Setup(string content)
    {
        EmulatorControlService.Setup(content);
    }

    public void Start()
    {
        EmulatorControlService.Start();
    }

    public void Stop()
    {
        EmulatorControlService.Stop();
    }

    public void Pause()
    {
        EmulatorControlService.Pause();
    }

    public void Continue()
    {
        EmulatorControlService.Continue();
    }

    public void Frame()
    {
        EmulatorControlService.Frame();
    }

    #endregion
}