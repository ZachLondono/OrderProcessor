using ApplicationCore.Shared;
using DesktopHost.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApplicationCore;
using MediatR;
using System;
using System.Windows;
using ApplicationCore.Features.CLI;

namespace DesktopHost;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

    private async void Application_Startup(object sender, StartupEventArgs e) {

        var configuration = BuildConfiguration();
        var serviceProvider = BuildServiceProvider(configuration);

        if (e.Args.Length > 0) {
            try {
                var app = serviceProvider.GetRequiredService<ConsoleApplication>();
                await app.Run(e.Args);
            } catch (Exception ex) {
                MessageBox.Show($"Error loading order\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Shutdown();
            return;
        }

        new MainWindow(serviceProvider).Show();

    }

    private static IConfiguration BuildConfiguration()
        => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

    private static IServiceProvider BuildServiceProvider(IConfiguration configuration) {
        var services = new ServiceCollection()
                            .AddMediatR(typeof(MainWindow))
                            .AddApplicationCoreServices(configuration)
                            .AddSingleton<IFilePicker, WPFDialogFilePicker>()
                            .AddSingleton<IMessageBoxService, WPFMessageBox>()
                            .AddSingleton<ConsoleApplication>()
                            .AddSingleton(configuration)
                            .AddLogging(ConfigureLogging);

        services.AddWpfBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        return services.BuildServiceProvider();

    }

    private static void ConfigureLogging(ILoggingBuilder loggingBuilder) {
        loggingBuilder.AddDebug();
#if DEBUG
        loggingBuilder.AddFilter("ApplicationCore", LogLevel.Trace);
#else
        loggingBuilder.AddFilter("ApplicationCore", LogLevel.Information);
#endif
    }


}
