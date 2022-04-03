using PSX.Frontend.Core.Services;
using PSX.Frontend.Core.Services.Emulator;

namespace PSX.Frontend.Core.Interface;

public sealed class MainModel
    : IEmulatorControlService // for view model commands
{
    public MainModel(
        IApplicationService applicationService,
        IEmulatorControlService emulatorControlService,
        IFileService fileService
    )
    {
        ApplicationService     = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
        EmulatorControlService = emulatorControlService ?? throw new ArgumentNullException(nameof(emulatorControlService));
        FileService            = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    private IApplicationService ApplicationService { get; }

    private IEmulatorControlService EmulatorControlService { get; }

    private IFileService FileService { get; }

    public void OpenFile()
    {
        const string filter = "Everything|*.exe;*.psx;*.bin;*.cue|Application|*.exe;*.psx|Image|*.bin;*.cue";

        var path = FileService.OpenFile(filter);

        if (path is null)
            return;

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