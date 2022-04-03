using PSX.Frontend.Core.Services;
using PSX.Frontend.Core.Services.Emulator;

namespace PSX.Frontend.Core.Interface;

public sealed class MainModel : IEmulatorServiceController
{
    public MainModel(IApplicationService applicationService, IEmulatorService emulatorService, IFileService fileService)
    {
        ApplicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
        EmulatorService    = emulatorService ?? throw new ArgumentNullException(nameof(emulatorService));
        FileService        = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    private IApplicationService ApplicationService { get; }

    private IEmulatorService EmulatorService { get; }

    private IFileService FileService { get; }

    public void OpenFile()
    {
        const string filter = "Everything|*.exe;*.psx;*.bin;*.cue|Application|*.exe;*.psx|Image|*.bin;*.cue";

        var path = FileService.OpenFile(filter);

        if (path is null)
            return;

        EmulatorService.Setup(path);
    }

    public void Shutdown()
    {
        ApplicationService.Shutdown();
    }

    #region IEmulatorPlayer

    public bool CanStart => EmulatorService.CanStart;

    public bool CanStop => EmulatorService.CanStop;

    public bool CanPause => EmulatorService.CanPause;

    public bool CanContinue => EmulatorService.CanContinue;

    public bool CanFrame => EmulatorService.CanFrame;

    public void Setup(string content)
    {
        EmulatorService.Setup(content);
    }

    public void Start()
    {
        EmulatorService.Start();
    }

    public void Stop()
    {
        EmulatorService.Stop();
    }

    public void Pause()
    {
        EmulatorService.Pause();
    }

    public void Continue()
    {
        EmulatorService.Continue();
    }

    public void Frame()
    {
        EmulatorService.Frame();
    }

    #endregion
}