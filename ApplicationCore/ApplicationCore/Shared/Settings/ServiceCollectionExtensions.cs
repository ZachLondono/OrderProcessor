using ApplicationCore.Shared.Settings.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Shared.Settings;

public static class Extensions {

    public static IConfigurationBuilder AddSettingsFiles(this IConfigurationBuilder builder) {

        string configDirectory;
#if DEBUG
        configDirectory = "Configuration";
#else
        configDirectory = @"C:\ProgramData\OrderProcessor\Configuration";
#endif

        string[] configFiles = new string[] {
            "paths.json",
            "email.json",
            "pdfconfig.json",
            "tools.json",
#if DEBUG
            "data.Development.json"
#else
            "data.json"
#endif
        };

        foreach (var fileName in configFiles) {

            var finalPath = Path.Combine(configDirectory, fileName);

#if !DEBUG
            if (!File.Exists(finalPath)) {
                File.Copy(Path.Combine("Configuration", fileName), finalPath);
            }
#endif

            builder.AddJsonFile(finalPath, optional: false, reloadOnChange: true);

        }

        return builder;

    }

    public static IServiceCollection ConfigureSettings(this IServiceCollection services, IConfiguration configuration) {

        services.ConfigureWritable<DataFilePaths>(configuration.GetRequiredSection("data"), "Configuration\\data.json");
        services.ConfigureWritable<ToolConfiguration>(configuration.GetRequiredSection("tools"), "Configuration\\tools.json");
        services.Configure<ConfigurationFiles>(configuration.GetRequiredSection("ConfigurationFiles"));
        services.Configure<Paths>(configuration.GetRequiredSection("paths"));
        services.Configure<Email>(configuration.GetRequiredSection("Email"));

        return services;

    }

    public static IServiceCollection ConfigureWritable<T>(this IServiceCollection services, IConfigurationSection section, string file = "appsettings.json") where T : class, new() {

        return services
            .Configure<T>(section)
            .AddTransient<IWritableOptions<T>>(provider => {
                var options = provider.GetRequiredService<IOptionsMonitor<T>>();
                return new WritableOptions<T>(options, section.Key, file);
            });

    }
}
