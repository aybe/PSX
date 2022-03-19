namespace PSX.Core.Extensions;

public static class QueueExtensions
{
    public static void EnqueueRange<T>(this Queue<T> queue, Span<T> parameters)
    {
        if (queue == null)
            throw new ArgumentNullException(nameof(queue));

        foreach (var parameter in parameters)
        {
            queue.Enqueue(parameter);
        }
    }
}