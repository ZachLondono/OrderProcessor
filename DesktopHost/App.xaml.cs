using ApplicationCore.Shared;
using DesktopHost.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApplicationCore;
using MediatR;
using System;
using System.Windows;
using ApplicationCore.Infrastructure;
using DesktopHost.Services;

namespace DesktopHost;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

    private void Application_Startup(object sender, StartupEventArgs e) {

        var configuration = BuildConfiguration();
        var serviceProvider = BuildServiceProvider(configuration);

        if (e.Args.Length == 1) {
            try {
                TryLoadOrderFromFile(serviceProvider, e.Args[0]);
            } catch (Exception ex) {
                MessageBox.Show($"Error loading order\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Shutdown();
            return;
        }

        new MainWindow(serviceProvider).Show();

    }

    private static void TryLoadOrderFromFile(IServiceProvider serviceProvider, string filePath) {
        var bus = serviceProvider.GetRequiredService<IBus>();
        var messageBox = serviceProvider.GetRequiredService<IMessageBoxService>();

        var loader = new LoadOrderFromExcelService(bus, messageBox);

        var response = loader.LoadOrder(filePath);

        string content = "Nothing happend";
        response.Match(
            order => content = $"Order loaded: \n{order.Name}\n{order.Id}",
            error => content = "Error loading order \n" + error.Message
        );

        messageBox.OpenDialog(content, "Load order result");
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
