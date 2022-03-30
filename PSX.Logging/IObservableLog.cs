namespace PSX.Logging;

public interface IObservableLog
{
    ObservableQueue<string> Entries { get; }
}