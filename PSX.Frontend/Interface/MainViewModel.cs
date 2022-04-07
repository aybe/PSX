using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using PSX.Frontend.Services.Emulation;

namespace PSX.Frontend.Interface;

public sealed class MainViewModel : ObservableRecipient
{
    private readonly IEmulatorDisplayService EmulatorDisplayService;

    public MainViewModel(
        MainViewModelCommands commands,
        IEmulatorDisplayService emulatorDisplayService,
        IOptions<AppSettings> appSettings)
    {
        Commands               = commands;
        EmulatorDisplayService = emulatorDisplayService;
        AppSettings            = appSettings;
    }

    public IOptions<AppSettings> AppSettings { get; }

    public MainViewModelCommands Commands { get; }

    protected override void OnActivated()
    {
        base.OnActivated();

        EmulatorDisplayService.UpdateAudioDataHandlers.Add(UpdateAudioData);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        EmulatorDisplayService.UpdateAudioDataHandlers.Remove(UpdateAudioData);
    }

    private void UpdateAudioData(UpdateAudioDataMessage message)
    {
        // TODO
    }
}