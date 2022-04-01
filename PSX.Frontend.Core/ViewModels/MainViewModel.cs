using System.Diagnostics.CodeAnalysis;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PSX.Frontend.Core.Models;
using PSX.Frontend.Core.Navigation;
using PSX.Frontend.Core.Views;

namespace PSX.Frontend.Core.ViewModels;

public sealed class MainViewModel : ObservableRecipient
{
    public MainViewModel(MainModel model, INavigationService navigationService)
    {
        Model = model;

        NavigationService = navigationService;

        OpenFile = new RelayCommand(OpenFileExecute, OpenFileCanExecute);

        Shutdown = new RelayCommand(ShutdownExecute, ShutdownCanExecute);

        OpenOutput = new RelayCommand(OpenOutputExecute, OpenOutputCanExecute);

        OpenLogging = new RelayCommand(OpenLoggingExecute, OpenLoggingCanExecute);
    }

    private MainModel Model { get; }

    private INavigationService NavigationService { get; }

    #region OpenOutput

    public RelayCommand OpenOutput { get; }

    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    private bool OpenOutputCanExecute()
    {
        return true;
    }

    private void OpenOutputExecute()
    {
        NavigationService.Navigate<IOutputView>();
    }

    #endregion

    #region OpenLogging

    public RelayCommand OpenLogging { get; }

    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    private bool OpenLoggingCanExecute()
    {
        return true;
    }

    private void OpenLoggingExecute()
    {
        NavigationService.Navigate<ILoggingView>();
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