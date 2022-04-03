using PSX.Frontend.Core.Services;
using PSX.Frontend.Core.Services.Emulator;

namespace PSX.Frontend.Core.Interface;

public sealed class MainModel : IEmulatorPlayer
{
    private readonly IEmulatorPlayer Player = new EmulatorPlayer(); // BUG use DI

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

        Player.Setup(EmulatorService, path);
    }

    public void Shutdown()
    {
        ApplicationService.Shutdown();
    }

    #region IEmulatorPlayer

    public bool CanStart => Player.CanStart;

    public bool CanStop => Player.CanStop;

    public bool CanPause => Player.CanPause;

    public bool CanContinue => Player.CanContinue;

    public bool CanFrame => Player.CanFrame;

    public void Setup(IEmulatorService service, string content)
    {
        Player.Setup(service, content);
    }

    public void Start()
    {
        Player.Start();
    }

    public void Stop()
    {
        Player.Stop();
    }

    public void Pause()
    {
        Player.Pause();
    }

    public void Continue()
    {
        Player.Continue();
    }

    public void Frame()
    {
        Player.Frame();
    }

    #endregion
}