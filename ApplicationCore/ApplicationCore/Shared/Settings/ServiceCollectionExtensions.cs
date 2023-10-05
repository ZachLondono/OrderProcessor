using ApplicationCore.Shared.Settings.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Shared.Settings;

public static class Extensions {

#if DEBUG
    private static string _configDirectory = "Configuration";
#else
    private static string _configDirectory = @"C:\ProgramData\OrderProcessor\Configuration";
#endif

    private static string[] _configFiles = new string[] {
            "schedule.json",
            "email.json",
            "pdfconfig.json",
            "tools.json",
            "data.json",
            "data.Development.json"
    };

    public static IConfigurationBuilder AddSettingsFiles(this IConfigurationBuilder builder) {

        foreach (var fileName in _configFiles) {

            var finalPath = Path.Combine(_configDirectory, fileName);

#if !DEBUG
            if (!File.Exists(finalPath)) {
                File.Copy(Path.Combine(@".\Configuration", fileName), finalPath);
            }
#endif

            builder.AddJsonFile(finalPath, optional: false, reloadOnChange: true);

        }

        return builder;

    }

    public static IServiceCollection ConfigureSettings(this IServiceCollection services, IConfiguration configuration) {

#if DEBUG
        services.ConfigureWritable<DataFilePaths>(configuration.GetRequiredSection("data"), Path.Combine(_configDirectory, "data.Development.json"));
#else
        services.ConfigureWritable<DataFilePaths>(configuration.GetRequiredSection("data"), Path.Combine(_configDirectory, "data.json"));
#endif

        services.ConfigureWritable<ToolConfiguration>(configuration.GetRequiredSection("tools"), Path.Combine(_configDirectory, "tools.json"));
        services.ConfigureWritable<EmailSettings>(configuration.GetRequiredSection("Email"), Path.Combine(_configDirectory, "email.json"));
        services.ConfigureWritable<ScheduleSettings>(configuration.GetRequiredSection("schedule"), Path.Combine(_configDirectory, "schedule.json"));
        services.ConfigureWritable<ExportSettings>(configuration.GetRequiredSection("ExportSettings"));

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
