using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PSX.Frontend.Core.Models;

namespace PSX.Frontend.Core.ViewModels;

public sealed class MainViewModel : ObservableRecipient
{
    public MainViewModel()
    {
        OpenFile = new RelayCommand(OpenFileExecute, OpenFileCanExecute);
        Shutdown = new RelayCommand(ShutdownExecute, ShutdownCanExecute);
    }

    private MainModel Model { get; } = new();

    #region OpenFile

    public RelayCommand OpenFile { get; }

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