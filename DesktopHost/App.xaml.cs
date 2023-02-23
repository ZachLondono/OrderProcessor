using DesktopHost.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApplicationCore;
using MediatR;
using System;
using System.Windows;
using ApplicationCore.Features.Shared.Services;
using DesktopHost.Error;

namespace DesktopHost;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

    private void Application_Startup(object sender, StartupEventArgs e) {

        try {

            var configuration = BuildConfiguration();
            var serviceProvider = BuildServiceProvider(configuration);

            new MainWindow(serviceProvider).Show();

        } catch (Exception ex) {

            new ErrorWindow {
                DataContext = new ErrorWindowViewModel() {
                    Title = "Error Initilizing Application",
                    Message = ex.Message
                }
            }.Show();

        }

    }

    private static IConfiguration BuildConfiguration()
        => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("Configuration/credentials.json")
                .Build();

    private static IServiceProvider BuildServiceProvider(IConfiguration configuration) {
        var services = new ServiceCollection()
                            .AddMediatR(typeof(MainWindow))
                            .AddApplicationCoreServices(configuration)
                            .AddSingleton<IFilePicker, WPFDialogFilePicker>()
                            .AddSingleton<IMessageBoxService, WPFMessageBox>()
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
