using System;
using MediatR;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApplicationCore;
using DesktopHost.Dialogs;
using ApplicationCore.Shared;
using Microsoft.AspNetCore.Components.WebView;

namespace DesktopHost;

public partial class MainWindow : Window {
    
    public MainWindow() {
        InitializeComponent();

        var configuration = BuildConfiguration();
        var serviceProvider = BuildServiceProvider(configuration);

        blazorWebView.UrlLoading +=
                    (sender, urlLoadingEventArgs) => {
                        if (urlLoadingEventArgs.Url.Host != "0.0.0.0") {
                            urlLoadingEventArgs.UrlLoadingStrategy = UrlLoadingStrategy.OpenExternally;
                        }
                    };

        Resources.Add("services", serviceProvider);

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
