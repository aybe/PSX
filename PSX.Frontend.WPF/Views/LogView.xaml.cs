using System;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Frontend.Core.Old.Messages;
using PSX.Frontend.Core.Old.Models;

namespace PSX.Frontend.WPF.Views;

internal sealed partial class LogView : ILogView
{
    public LogView(LogViewModel model)
    {
        InitializeComponent();

        DataContext = model ?? throw new ArgumentNullException(nameof(model));

        Loaded += (_, _) =>
        {
            model.IsActive = true;
        };

        Closed += (_, _) =>
        {
            model.IsActive = false;
        };

        WeakReferenceMessenger.Default.Register<ViewActivationMessage>(this, (recipient, _) =>
        {
            // activate ourselves when asked to do so, see ShellViewModel.OpenLogExecute for the full story
            var instance = recipient as LogView ?? throw new ArgumentOutOfRangeException(nameof(recipient));
            instance.Activate();
        });
    }
}