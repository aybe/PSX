using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Frontend.Core.Messages;
using PSX.Logging;

namespace PSX.Frontend.Core.Models;

public sealed class LogViewModel : ObservableRecipient, IObservableLog
{
    public LogViewModel(IServiceProvider provider)
    {
        var service = provider.GetRequiredService<ILoggerProvider>();

        var log = service as IObservableLog ?? throw new InvalidOperationException();

        Entries = log.Entries;
    }

    public ObservableQueue<string> Entries { get; }

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