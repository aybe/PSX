using System;
using JetBrains.Annotations;
using PSX.Frontend.WPF.Frontend.Shared;
using PSX.Frontend.WPF.Interop;

namespace PSX.Frontend.WPF.Frontend;

[UsedImplicitly]
internal sealed class MainModelCommands 
{
    private MainModel Model { get; }

    public MainModelCommands(MainModel model) 
    {
        Model              = model ?? throw new ArgumentNullException(nameof(model));

        OpenExecutable     = new RelayCommand(OpenExecutableExecute,     OpenExecutableCanExecute);
        OpenVideoOutput    = new RelayCommand(OpenVideoOutputExecute,    OpenVideoOutputCanExecute);
        OpenVideoMemory    = new RelayCommand(OpenVideoMemoryExecute,    OpenVideoMemoryCanExecute);
        OpenConsole        = new RelayCommand(OpenConsoleExecute,        OpenConsoleCanExecute);
        EmulationStart     = new RelayCommand(EmulationStartExecute,     EmulationStartCanExecute);
        EmulationPause     = new RelayCommand(EmulationPauseExecute,     EmulationPauseCanExecute);
        EmulationContinue  = new RelayCommand(EmulationContinueExecute,  EmulationContinueCanExecute);
        EmulationTerminate = new RelayCommand(EmulationTerminateExecute, EmulationTerminateCanExecute);
        About              = new RelayCommand(AboutExecute,              AboutCanExecute);
        Quit               = new RelayCommand(QuitExecute,               QuitCanExecute);
    }

    #region OpenExecutable

    public RelayCommand OpenExecutable { get; }

    private bool OpenExecutableCanExecute()
    {
        return true;
    }

    private void OpenExecutableExecute()
    {
        Model.Open();
    }

    #endregion

    #region OpenVideoOutput

    public RelayCommand OpenVideoOutput { get; }

    private bool OpenVideoOutputCanExecute()
    {
        return true;
    }

    private void OpenVideoOutputExecute()
    {
    }

    #endregion

    #region OpenVideoMemory

    public RelayCommand OpenVideoMemory { get; }

    private bool OpenVideoMemoryCanExecute()
    {
        return true;
    }

    private void OpenVideoMemoryExecute()
    {
    }

    #endregion

    #region OpenConsole

    public RelayCommand OpenConsole { get; }

    private bool OpenConsoleCanExecute()
    {
        return Model.CanOpen;
    }

    private void OpenConsoleExecute()
    {
        NativeMethods.AllocConsole();
    }

    #endregion

    #region EmulationStart

    public RelayCommand EmulationStart { get; }

    private bool EmulationStartCanExecute()
    {
        return Model.CanStart;
    }

    private void EmulationStartExecute()
    {
        Model.Start();
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
    }

    #endregion

    #region EmulationTerminate

    public RelayCommand EmulationTerminate { get; }

    private bool EmulationTerminateCanExecute()
    {
        return Model.CanTerminate;
    }

    private void EmulationTerminateExecute()
    {
        Model.Terminate();
    }

    #endregion

    #region About

    public RelayCommand About { get; }

    private bool AboutCanExecute()
    {
        return true;
    }

    private void AboutExecute()
    {
        // TODO
    }

    #endregion

    #region Quit

    public RelayCommand Quit { get; }

    private bool QuitCanExecute()
    {
        return true;
    }

    private void QuitExecute()
    {
        App.Current.Shutdown(); // TODO cleanup, etc...
    }

    #endregion
}