using System.Diagnostics.CodeAnalysis;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PSX.Frontend.Core.Models;

namespace PSX.Frontend.Core.ViewModels;

public sealed class MainViewModel : ObservableRecipient
{
    public MainViewModel(MainModel model)
    {
        Model = model;

        OpenFile = new RelayCommand(OpenFileExecute, OpenFileCanExecute);

        Shutdown = new RelayCommand(ShutdownExecute, ShutdownCanExecute);
    }

    private MainModel Model { get; }

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