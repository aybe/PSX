using Microsoft.Extensions.Options;

namespace PSX.Frontend.Services.Options;

/// <inheritdoc />
public interface IWritableOptions<out T> : IOptions<T> where T : class
{
    public void Update(Action<T>? action = null);
}