using PSX.Frontend.Services;
using PSX.Frontend.Services.Emulation;

namespace PSX.Frontend.Interface;

public sealed class MainModel
    : IEmulatorControlService // for view model commands
{
    public MainModel(
        IApplicationService     applicationService,
        IEmulatorControlService emulatorControlService
    )
    {
        ApplicationService     = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
        EmulatorControlService = emulatorControlService ?? throw new ArgumentNullException(nameof(emulatorControlService));
    }

    private IApplicationService ApplicationService { get; }

    private IEmulatorControlService EmulatorControlService { get; }

    public void OpenFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));

        if (File.Exists(path) is false)
            throw new FileNotFoundException(null, path);

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