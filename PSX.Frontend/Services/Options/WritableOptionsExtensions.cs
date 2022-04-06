using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace PSX.Frontend.Services.Options;

public static class WritableOptionsExtensions
{
    public static IServiceCollection ConfigureWritable<T>(
        this IServiceCollection services, IConfigurationSection section, string fileName = "AppSettings.json")
        where T : class, new()
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        if (section is null)
            throw new ArgumentNullException(nameof(section));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));

        services
            .Configure<T>(section)
            .AddTransient<IWritableOptions<T>>(s =>
            {
                var cfg = s.GetRequiredService<IConfigurationRoot>();
                var env = s.GetRequiredService<IHostEnvironment>();
                var mon = s.GetRequiredService<IOptionsMonitor<T>>();

                return new WritableOptions<T>(env, mon, cfg, section.Key, fileName);
            });

        return services;
    }
}