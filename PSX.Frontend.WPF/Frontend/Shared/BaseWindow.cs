using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectPSX.WPF.Frontend.Shared;

public class BaseWindow<T> : Window where T : BaseModel
{
    protected BaseWindow()
    {
        DataContext = Model = App.Current.Services.GetService<T>() ?? throw new InvalidOperationException();

        Loaded += OnLoaded;

        Closed += OnClosed;
    }

    private T Model { get; }

    protected virtual void OnLoaded(object sender, RoutedEventArgs e)
    {
        Model.IsActive = true;
    }

    protected virtual void OnClosed(object? sender, EventArgs e)
    {
        Model.IsActive = false;
    }
}