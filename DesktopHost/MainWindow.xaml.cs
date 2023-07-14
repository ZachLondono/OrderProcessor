using System;
using System.Windows;
using Microsoft.AspNetCore.Components.WebView;
using Windows.ApplicationModel;

namespace DesktopHost;

public partial class MainWindow : Window {

    public MainWindow(IServiceProvider serviceProvider) {
        InitializeComponent();

        blazorWebView.UrlLoading +=
                    (sender, urlLoadingEventArgs) => {
                        if (urlLoadingEventArgs.Url.Host != "0.0.0.0") {
                            urlLoadingEventArgs.UrlLoadingStrategy = UrlLoadingStrategy.OpenExternally;
                        }
                    };

        Resources.Add("services", serviceProvider);

        Title = GetTitle();

    }

    private static string GetTitle() {

        string title = "";

        try {

            var version =  Package.Current.Id.Version;
            title = $"Royal Order Processor {version.Major}.{version.Minor}.{version.Build}";

        } catch {

            title = "Royal Order Processor";

        }

#if DEBUG
        title += " (DEBUG)";
#endif

        return title;

    }

}
