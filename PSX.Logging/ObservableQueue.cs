using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace PSX.Logging;

public class ObservableQueue<T> : Queue<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    public ObservableQueue(int capacity) : base(capacity)
    {
        Capacity = capacity;
    }

    public int Capacity { get; }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public new void Enqueue(T item)
    {
        var index = Count;
        base.Enqueue(item);
        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public void EnqueueRange(IEnumerable<T> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
        {
            base.Enqueue(item);
        }

        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public new T Dequeue()
    {
        var dequeue = base.Dequeue();
        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, dequeue, 0));
        return dequeue;
    }

    public new void Clear()
    {
        base.Clear();

        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public new bool TryDequeue([MaybeNullWhen(false)] out T result)
    {
        if (!base.TryDequeue(out result))
            return false;

        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, result, 0));
        return true;
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }
}