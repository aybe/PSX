using System.Diagnostics.CodeAnalysis;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PSX.Frontend.Services.Emulation;
using PSX.Frontend.Services.Navigation;
using PSX.Frontend.Services.Options;

namespace PSX.Frontend.Interface;

public sealed class MainViewModel : ObservableRecipient
{
    public MainViewModel(
        MainModel model,
        IEmulatorDisplayService emulatorDisplayService,
        INavigationService navigationService,
        IWritableOptions<AppSettings> appSettings)
    {
        Model = model;

        EmulatorDisplayService = emulatorDisplayService;

        NavigationService = navigationService;

        AppSettings = appSettings;

        OpenFile = new RelayCommand(OpenFileExecute, OpenFileCanExecute);

        OpenFileDirect = new RelayCommand<string>(OpenFileDirectExecute, OpenFileDirectCanExecute);

        Shutdown = new RelayCommand(ShutdownExecute, ShutdownCanExecute);

        OpenVideoScreen = new RelayCommand(OpenVideoScreenExecute, OpenVideoScreenCanExecute);

        OpenVideoMemory = new RelayCommand(OpenVideoMemoryExecute, OpenVideoMemoryCanExecute);

        EmulationStart    = new RelayCommand(EmulationStartExecute,    EmulationStartCanExecute);
        EmulationStop     = new RelayCommand(EmulationStopExecute,     EmulationStopCanExecute);
        EmulationPause    = new RelayCommand(EmulationPauseExecute,    EmulationPauseCanExecute);
        EmulationFrame    = new RelayCommand(EmulationFrameExecute,    EmulationFrameCanExecute);
        EmulationContinue = new RelayCommand(EmulationContinueExecute, EmulationContinueCanExecute);
    }

    private MainModel Model { get; }

    private IEmulatorDisplayService EmulatorDisplayService { get; }

    private INavigationService NavigationService { get; }

    public IWritableOptions<AppSettings> AppSettings { get; }

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

    #region EmulationStart

    public RelayCommand EmulationStart { get; }

    private bool EmulationStartCanExecute()
    {
        return Model.CanStart;
    }

    private void EmulationStartExecute()
    {
        Model.Start();

        EmulationStart.NotifyCanExecuteChanged();
        EmulationStop.NotifyCanExecuteChanged();
        EmulationPause.NotifyCanExecuteChanged();
        EmulationContinue.NotifyCanExecuteChanged();
        EmulationFrame.NotifyCanExecuteChanged();
    }

    #endregion

    #region EmulationStop

    public RelayCommand EmulationStop { get; }

    private bool EmulationStopCanExecute()
    {
        return Model.CanStop;
    }

    private void EmulationStopExecute()
    {
        Model.Stop();

        EmulationStart.NotifyCanExecuteChanged();
        EmulationStop.NotifyCanExecuteChanged();
    }

    #endregion

    #region EmulationPause

    public RelayCommand EmulationPause { get; }

    private bool EmulationPauseCanExecute()
    {
        return Model.CanPause;
    }

    private void EmulationPauseExecute()
    {
        Model.Pause();

        EmulationPause.NotifyCanExecuteChanged();
        EmulationFrame.NotifyCanExecuteChanged();
        EmulationContinue.NotifyCanExecuteChanged();
    }

    #endregion

    #region EmulationFrame

    public RelayCommand EmulationFrame { get; }

    private bool EmulationFrameCanExecute()
    {
        return Model.CanFrame;
    }

    private void EmulationFrameExecute()
    {
        Model.Frame();

        EmulationPause.NotifyCanExecuteChanged();
        EmulationFrame.NotifyCanExecuteChanged();
        EmulationContinue.NotifyCanExecuteChanged();
    }

    #endregion

    #region EmulationContinue

    public RelayCommand EmulationContinue { get; }

    private bool EmulationContinueCanExecute()
    {
        return Model.CanContinue;
    }

    private void EmulationContinueExecute()
    {
        Model.Continue();

        EmulationPause.NotifyCanExecuteChanged();
        EmulationFrame.NotifyCanExecuteChanged();
        EmulationContinue.NotifyCanExecuteChanged();
    }

    #endregion

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
        EmulationStart.NotifyCanExecuteChanged();
        EmulationStop.NotifyCanExecuteChanged();
        EmulationPause.NotifyCanExecuteChanged();
        EmulationFrame.NotifyCanExecuteChanged();
        EmulationContinue.NotifyCanExecuteChanged();
    }

    #endregion

    #region OpenFileDirect

    public RelayCommand<string> OpenFileDirect { get; }

    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    private bool OpenFileDirectCanExecute(string? path)
    {
        return File.Exists(path);
    }

    private void OpenFileDirectExecute(string? path)
    {
        throw new NotImplementedException();
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