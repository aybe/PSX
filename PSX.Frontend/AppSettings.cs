using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSX.Frontend;

public sealed class AppSettings
{
    public ObservableCollection<string> RecentlyUsed { get; } = new();

    public int RecentlyUsedLength { get; set; } = 10;

    public void AddToRecentlyUsed(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));

        for (var i = RecentlyUsed.Count - 1; i >= 0; i--)
        {
            if (string.Equals(RecentlyUsed[i], path, StringComparison.OrdinalIgnoreCase))
            {
                RecentlyUsed.RemoveAt(i);
            }
        }

        RecentlyUsed.Insert(0, path);

        while (RecentlyUsed.Count > RecentlyUsedLength)
        {
            RecentlyUsed.RemoveAt(RecentlyUsed.Count - 1);
        }
    }

    public void Save()
    {
        var services = AppStartup.Instance?.Host.Services ?? throw new InvalidOperationException();

        // get the path where settings should be written to

        var configuration = services.GetRequiredService<IConfiguration>();
        if (configuration is not IConfigurationRoot configurationRoot)
            throw new InvalidOperationException();

        var provider = configurationRoot.Providers.OfType<JsonConfigurationProvider>().Single();
        var fileInfo = provider.Source.FileProvider.GetFileInfo(provider.Source.Path);

        // only update relevant node in file as it may contain stuff unknown to us

        var text = File.ReadAllText(fileInfo.PhysicalPath);
        var tree = JsonConvert.DeserializeObject<JObject>(text) ?? throw new InvalidOperationException();
        var data = JsonConvert.SerializeObject(this);
        var node = JObject.Parse(data);

        tree[nameof(AppSettings)] = node;

        var json = JsonConvert.SerializeObject(tree, Formatting.Indented);

        File.WriteAllText(fileInfo.PhysicalPath, json);
    }

    public void Update(Action<AppSettings>? action = null)
    {
        action?.Invoke(this);

        Save();
    }
}