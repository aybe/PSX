using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSX.Frontend.Services.Options;

internal sealed class WritableOptions<T> : IWritableOptions<T> where T : class, new()
{
    public WritableOptions(
        IHostEnvironment environment,
        IOptionsMonitor<T> monitor,
        IConfigurationRoot root,
        string section,
        string fileName
    )
    {
        Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        Monitor     = monitor ?? throw new ArgumentNullException(nameof(monitor));
        Root        = root ?? throw new ArgumentNullException(nameof(root));
        Section     = !string.IsNullOrWhiteSpace(section) ? section : throw new ArgumentException("Value cannot be null or whitespace.",   nameof(section));
        FileName    = !string.IsNullOrWhiteSpace(fileName) ? fileName : throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));
    }

    private IHostEnvironment Environment { get; }

    private IOptionsMonitor<T> Monitor { get; }

    private IConfigurationRoot Root { get; }

    private string Section { get; }

    private string FileName { get; }

    public T Value => Monitor.CurrentValue;

    public void Update(Action<T>? action = null)
    {
        var info = Environment.ContentRootFileProvider.GetFileInfo(FileName);
        var path = info.PhysicalPath;
        var json = File.ReadAllText(path);

        var tree = JsonConvert.DeserializeObject<JObject>(json) ?? throw new InvalidOperationException();
        var data = tree.TryGetValue(Section, out var section)
            ? JsonConvert.DeserializeObject<T>(section.ToString())
            : Value;

        data ??= new T();

        action?.Invoke(data);

        tree[Section] = JObject.Parse(JsonConvert.SerializeObject(data));

        File.WriteAllText(path, JsonConvert.SerializeObject(tree, Formatting.Indented));

        Root.Reload();
    }
}