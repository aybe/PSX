using System;
using System.Windows.Input;

namespace ProjectPSX.WPF.Frontend.Shared;

internal sealed class RelayCommand : ICommand
    // because their implementation totally sucks https://github.com/CommunityToolkit/MVVM-Samples/issues/41
{
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        Execute    = execute ?? throw new ArgumentNullException(nameof(execute));
        CanExecute = canExecute;
    }

    private Action Execute { get; }

    private Func<bool>? CanExecute { get; }

    bool ICommand.CanExecute(object? parameter)
    {
        return CanExecute is null || CanExecute();
    }

    void ICommand.Execute(object? parameter)
    {
        Execute();
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}