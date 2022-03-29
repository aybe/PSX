using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Frontend.Core.Messages;
using PSX.Logging;

namespace PSX.Frontend.Core.Modules;

public sealed class ViewModelLog : ObservableRecipient, IObservableLog
{
    public ViewModelLog(IServiceProvider provider)
    {
        var service = provider.GetRequiredService<ILoggerProvider>();

        var log = service as IObservableLog ?? throw new InvalidOperationException();

        Entries = log.Entries;
    }

    public ObservableCollection<LogEntry>? Entries { get; }

    protected override void OnActivated()
    {
        base.OnActivated();

        WeakReferenceMessenger.Default.Send(new ViewVisibilityMessage(ViewVisibility.Visible), Tokens.LogShell); // for menu command
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        WeakReferenceMessenger.Default.Send(new ViewVisibilityMessage(ViewVisibility.Hidden), Tokens.LogShell); // for menu command
    }
}