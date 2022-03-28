using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PSX.Logging;

namespace PSX.Frontend.Core.ViewModels;

public sealed class MainViewModel : ObservableRecipient, IObservableLogEntries
{
    public MainViewModel(IOptions<AppSettings> options, ILogger<MainViewModel> logger, IServiceProvider serviceProvider)
    {
        Options = options;
        Logger  = logger;

        var service = serviceProvider.GetService<ILoggerProvider>();

        if (service is IObservableLogEntries entries)
        {
            Entries = entries.Entries;
        }

        LogSomething = new RelayCommand(() => { Logger.LogInformation("From MainViewModel: {Time}", DateTime.Now.ToString(CultureInfo.InvariantCulture)); });
    }

    public ICommand LogSomething { get; }

    public IOptions<AppSettings> Options { get; }

    public ILogger<MainViewModel> Logger { get; }

    public string SomethingFromAppSettings => $"{Options.Value.Executable} from VM";

    public ObservableCollection<LogEntry>? Entries { get; }
}