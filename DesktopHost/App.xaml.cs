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
using ApplicationCore.Shared.Settings;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Linq;
using Serilog.Events;
using ApplicationCore;
using ApplicationCore.Features.CustomizationScriptManager;
using DesktopHost.ScriptEditor;
using ApplicationCore.Shared;

namespace DesktopHost;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

    [DllImport("user32")]
    public static extern int FlashWindow(IntPtr hwnd, bool bInvert);

    private void Application_Startup(object sender, StartupEventArgs e) {

        bool verboseLogging = e.Args.Contains("-v");

        var initState = new InitializationState() {
            JolaMode = e.Args.Contains("-jola")
        };

        Current.DispatcherUnhandledException += AppDispatcherUnhandledException;

        try {

            CreateLogger(verboseLogging);
            var configuration = BuildConfiguration();
            var serviceProvider = BuildServiceProvider(configuration, initState);

            new MainWindow(serviceProvider).Show();

        } catch (Exception ex) {

            var errorWindow = new ErrorWindow {
                DataContext = new ErrorWindowViewModel() {
                    Title = "Error Initializing Application",
                    Message = ex.Message
                }
            };

            errorWindow.Show();
            var wih = new WindowInteropHelper(errorWindow);
            _ = FlashWindow(wih.Handle, true);

            try {
                Log.Error(ex, "Exception thrown while trying to initialize application");
            } catch { }

        }

    }

    private static IConfiguration BuildConfiguration() {
        LogVerbose("Building configuration");
        return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#endif
                .AddJsonFile("Configuration/credentials.json", optional: false, reloadOnChange: true)
                .AddSettingsFiles(LogVerbose)
                .Build();
    }

    private static IServiceProvider BuildServiceProvider(IConfiguration configuration, InitializationState initState) {

        LogVerbose("Building service provider");

        var services = new ServiceCollection()
                            .AddSingleton(initState)
                            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MainWindow).Assembly))
                            .AddApplicationCoreServices(configuration)
                            .AddSingleton<IFilePicker, FilePicker>()
                            .AddSingleton<IMessageBoxService, WPFMessageBox>()
                            .AddTransient<IScriptEditorOpener, ScriptEditorOpener>()
                            .AddTransient<IWindowFocuser, WPFWindowFocuser>()
                            .AddSingleton(configuration)
                            .AddLogging(builder => builder.ClearProviders().AddSerilog())
                            .AddViewModels()
                            .ConfigureSettings(configuration, LogVerbose);

        services.AddWpfBlazorWebView();
        services.AddBlazorWebViewDeveloperTools();

        return services.BuildServiceProvider();

    }

    private static void CreateLogger(bool verbose) {

        LogEventLevel level;
        if (verbose) {
            level = LogEventLevel.Verbose;
        } else {
#if DEBUG
            level = LogEventLevel.Debug;
#else
            level = LogEventLevel.Information;
#endif
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Debug()
            .WriteTo.SQLite(@"C:\ProgramData\OrderProcessor\Logs\logs.db")
            .CreateLogger();
    }

    private static void LogVerbose(string message) {
        Log.Logger.Verbose(message);
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
