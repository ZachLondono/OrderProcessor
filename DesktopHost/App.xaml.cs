using DesktopHost.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Windows;
using ApplicationCore.Shared.Services;
using DesktopHost.Error;
using Serilog;
using System.Windows.Threading;
using ApplicationCore.Application;
using System.IO;
using System.Collections.Generic;

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
                    Title = "Error Initializing Application",
                    Message = ex.Message
                }
            }.Show();

        }

    }

    private static IConfiguration BuildConfiguration() {

        string configDirectory;
#if DEBUG
        configDirectory = "Configuration";
#else
        configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"OrderProcessor\Configuration");
#endif

        string[] configFiles = new string[] {
            "paths.json",
            "email.json",
            "pdfconfig.json",
#if DEBUG
            "data.Development.json"
#else
            "data.json"
#endif
        };

        var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("Configuration/credentials.json", optional: false, reloadOnChange: true);

        foreach (var fileName in configFiles) {

            var finalPath = Path.Combine(configDirectory, fileName);

#if !DEBUG
            if (!File.Exists(finalPath)) {
                File.Copy(Path.Combine("Configuration", fileName), finalPath);
            }
#endif

            builder.AddJsonFile(finalPath, optional: false, reloadOnChange: true);

        }

        return builder.Build();

    }

    private static IServiceProvider BuildServiceProvider(IConfiguration configuration) {
        var services = new ServiceCollection()
                            .AddMediatR(typeof(MainWindow))
                            .AddApplicationCoreServices(configuration)
                            .AddSingleton<IFilePicker, FilePicker>()
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
            .WriteTo.SQLite(@"C:\ProgramData\OrderProcessor\Logs\logs.db")
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
