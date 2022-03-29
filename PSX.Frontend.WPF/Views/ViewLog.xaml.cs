using System;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Frontend.Core.Messages;
using PSX.Frontend.Core.Modules;

namespace PSX.Frontend.WPF.Views;

internal sealed partial class ViewLog : IViewLog
{
    public ViewLog(ViewModelLog model)
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
            // activate ourselves when asked to do so, see ViewModelShell.OpenLogExecute for the full story
            var instance = recipient as ViewLog ?? throw new ArgumentOutOfRangeException(nameof(recipient));
            instance.Activate();
        });
    }
}