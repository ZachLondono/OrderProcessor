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
using Serilog;
using System.Windows.Threading;
using System.Diagnostics;

namespace DesktopHost;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

    private void Application_Startup(object sender, StartupEventArgs e) {

        Current.DispatcherUnhandledException += AppDispatcherUnhandledException;

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
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("Configuration/credentials.json", optional: false, reloadOnChange: true)
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
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .WriteTo.SQLite("logs.db")
            .CreateLogger();

        loggingBuilder.ClearProviders().AddSerilog();
    }

    private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
        runException(e.Exception);
        Log.Logger.Error(e.Exception, "Unhandled Exception");
        e.Handled = true;
    }

    void runException(Exception ex) {

        string message = string.Format(
                            "{0} Error:  {1}\r\n\r\n{2}",
                            ex.Source, ex.Message, ex.StackTrace);

        var window = new ErrorWindow {
            DataContext = new ErrorWindowViewModel() {
                Title = "Unhandled Exception in Application",
                Message = message
            }
        };

        window.ShowDialog();

        if (ex.InnerException != null) {
            runException(ex.InnerException);
        }
    }

}
