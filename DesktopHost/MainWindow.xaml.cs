using System;
using System.Windows;
using Microsoft.AspNetCore.Components.WebView;

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

    }

}
