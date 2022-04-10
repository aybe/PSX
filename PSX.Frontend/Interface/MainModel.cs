using PSX.Frontend.Services;
using PSX.Frontend.Services.Emulation;
using PSX.Frontend.Services.Navigation;

namespace PSX.Frontend.Interface;

public sealed class MainModel
    : IEmulatorControlService // for view model commands
{
    public MainModel(
        IApplicationService     applicationService,
        IEmulatorControlService emulatorControlService,
        INavigationService      navigationService
    )
    {
        ApplicationService     = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
        EmulatorControlService = emulatorControlService ?? throw new ArgumentNullException(nameof(emulatorControlService));
        NavigationService      = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    private IApplicationService ApplicationService { get; }

    private IEmulatorControlService EmulatorControlService { get; }

    private INavigationService NavigationService { get; }

    public void OpenFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));

        if (File.Exists(path) is false)
            throw new FileNotFoundException(null, path);

        // BUG atm if a view is opened after emu has started it will be blank as it hasn't received SetDisplayMode, should be cached

        NavigationService.Navigate<IVideoScreenView>(); // TODO remove this navigation to screen view once above is fixed

        EmulatorControlService.Setup(path);
        EmulatorControlService.Start();
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