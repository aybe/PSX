using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable RedundantAssignment
#pragma warning disable CS0162
#pragma warning disable IDE0059 // Unnecessary assignment of a value

namespace PSX.Logging;

public class ObservableLoggerCollection<T> : ObservableCollection<T>
{
    public void AddRange(IEnumerable<T> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        CheckReentrancy();

        var index1 = Items.Count;
        var index2 = index1;

        foreach (var item in items)
        {
            InsertItem(index1++, item);
        }

        OnPropertyChanged(RangeObservableCollectionProperties.CountPropertyChanged);

        OnPropertyChanged(RangeObservableCollectionProperties.IndexerPropertyChanged);

        // TODO https://docs.microsoft.com/en-us/archive/blogs/nathannesbit/addrange-and-observablecollection
        // TODO https://stackoverflow.com/questions/670577/observablecollection-doesnt-support-addrange-method-so-i-get-notified-for-each

        if (true)
        {
            OnCollectionChanged(RangeObservableCollectionProperties.ResetCollectionChanged);
        }
        else // BUG TODO System.NotSupportedException: 'Range actions are not supported.'
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    items as IList,
                    index2
                )
            );
        }
    }
}

internal static class RangeObservableCollectionProperties
{
    public static PropertyChangedEventArgs CountPropertyChanged { get; } = new("Count");

    public static PropertyChangedEventArgs IndexerPropertyChanged { get; } = new("Item[]");

    public static NotifyCollectionChangedEventArgs ResetCollectionChanged { get; } = new(NotifyCollectionChangedAction.Reset);
}