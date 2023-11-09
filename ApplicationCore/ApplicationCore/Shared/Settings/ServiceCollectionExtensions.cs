using ApplicationCore.Shared.Settings.CNC;
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
            "five_piece_doors.json",
            "doweled_drawer_boxes.json",
            "hafele_db_order.json",
            "cnc_settings.json",
            "data.json",
            "data.Development.json"
    };

    public static IConfigurationBuilder AddSettingsFiles(this IConfigurationBuilder builder, Action<string> logVerbose) {

        logVerbose($"Starting to add Json configuration files from directory: '{_configDirectory}'");

        var localConfigParentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        foreach (var fileName in _configFiles) {

            var finalPath = Path.Combine(_configDirectory, fileName);

#if !DEBUG
            if (!File.Exists(finalPath) && localConfigParentDir is not null) {
                var localFile = Path.Combine(localConfigParentDir, "Configuration", fileName);
                logVerbose($"Copying configuration file '{localFile}' to '{finalPath}'");
                File.Copy(localFile, finalPath);
            }
#endif

            logVerbose($"Adding json configuration file: {finalPath}");
            builder.AddJsonFile(finalPath, optional: false, reloadOnChange: true);

        }

        return builder;

    }

    public static IServiceCollection ConfigureSettings(this IServiceCollection services, IConfiguration configuration, Action<string> logVerbose) {

        logVerbose($"Adding settings to service provider from directory: '{_configDirectory}'");

#if DEBUG
        logVerbose($"Adding 'Development' version of settings to service provider");
        services.ConfigureWritable<DataFilePaths>(configuration.GetRequiredSection("data"), Path.Combine(_configDirectory, "data.Development.json"));
#else
        logVerbose($"Adding standard version of settings to service provider");
        services.ConfigureWritable<DataFilePaths>(configuration.GetRequiredSection("data"), Path.Combine(_configDirectory, "data.json"));
#endif

        services.ConfigureWritable<CNCSettings>(configuration.GetRequiredSection("cnc_settings"), Path.Combine(_configDirectory, "cnc_settings.json"));
        services.ConfigureWritable<HafeleDBOrderProviderSettings>(configuration.GetRequiredSection("hafele_db_order"), Path.Combine(_configDirectory, "hafele_db_order.json"));
        services.ConfigureWritable<FivePieceDoorCutListSettings>(configuration.GetRequiredSection("five_piece_doors"), Path.Combine(_configDirectory, "five_piece_doors.json"));
        services.ConfigureWritable<DoweledDrawerBoxCutListSettings>(configuration.GetRequiredSection("doweled_drawer_boxes"), Path.Combine(_configDirectory, "doweled_drawer_boxes.json"));
        services.ConfigureWritable<ToolConfiguration>(configuration.GetRequiredSection("tools"), Path.Combine(_configDirectory, "tools.json"));
        services.ConfigureWritable<EmailSettings>(configuration.GetRequiredSection("Email"), Path.Combine(_configDirectory, "email.json"));
        services.ConfigureWritable<ScheduleSettings>(configuration.GetRequiredSection("schedule"), Path.Combine(_configDirectory, "schedule.json"));
        services.ConfigureWritable<ExportSettings>(configuration.GetRequiredSection("ExportSettings"));
        services.Configure<PDFConfiguration>(configuration.GetRequiredSection("ReleasePDFConfig"));

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
