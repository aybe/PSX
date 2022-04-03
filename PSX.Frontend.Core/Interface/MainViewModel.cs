using System.Diagnostics.CodeAnalysis;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PSX.Frontend.Core.Services;
using PSX.Frontend.Core.Services.Emulator;
using PSX.Frontend.Core.Services.Navigation;

namespace PSX.Frontend.Core.Interface;

public sealed class MainViewModel : ObservableRecipient
{
    public MainViewModel(MainModel model, IEmulatorService emulatorService, INavigationService navigationService)
    {
        Model = model;

        EmulatorService = emulatorService;

        NavigationService = navigationService;

        OpenFile = new RelayCommand(OpenFileExecute, OpenFileCanExecute);

        Shutdown = new RelayCommand(ShutdownExecute, ShutdownCanExecute);

        OpenVideoScreen = new RelayCommand(OpenVideoScreenExecute, OpenVideoScreenCanExecute);

        OpenVideoMemory = new RelayCommand(OpenVideoMemoryExecute, OpenVideoMemoryCanExecute);
    }

    private MainModel Model { get; }

    private IEmulatorService EmulatorService { get; }

    private INavigationService NavigationService { get; }

    protected override void OnActivated()
    {
        base.OnActivated();

        EmulatorService.UpdateAudioDataMessageHandlers.Add(UpdateAudioData);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        EmulatorService.UpdateAudioDataMessageHandlers.Remove(UpdateAudioData);
    }

    private void UpdateAudioData(UpdateAudioDataMessage message)
    {
        throw new NotImplementedException();
    }

    #region OpenVideoScreen

    public RelayCommand OpenVideoScreen { get; }

    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    private bool OpenVideoScreenCanExecute()
    {
        return true;
    }

    private void OpenVideoScreenExecute()
    {
        NavigationService.Navigate<IVideoScreenView>();
    }

    #endregion

    #region OpenVideoMemory

    public RelayCommand OpenVideoMemory { get; }

    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    private bool OpenVideoMemoryCanExecute()
    {
        return true;
    }

    private void OpenVideoMemoryExecute()
    {
        NavigationService.Navigate<IVideoMemoryView>();
    }

    #endregion

    #region OpenFile

    public RelayCommand OpenFile { get; }

    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    private bool OpenFileCanExecute()
    {
        return true;
    }

    private void OpenFileExecute()
    {
        Model.OpenFile();
    }

    #endregion

    #region Shutdown

    public RelayCommand Shutdown { get; }

    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    private bool ShutdownCanExecute()
    {
        return true;
    }

    private void ShutdownExecute()
    {
        Model.Shutdown();
    }

    #endregion
}