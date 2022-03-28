using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using PSX.Logging;

namespace PSX.Frontend.WPF.Frontend.Views;

internal sealed class LogViewModel : ObservableRecipient, IObservableLog, ILogViewModel
{
    public LogViewModel(IServiceProvider provider)
    {
        var service = provider.GetRequiredService<ILoggerProvider>();

        var log = service as IObservableLog ?? throw new InvalidOperationException();

        Entries = log.Entries;
    }

    public ObservableCollection<LogEntry> Entries { get; }
}