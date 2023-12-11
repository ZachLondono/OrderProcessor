using ApplicationCore.Shared;
using System.Windows;

namespace DesktopHost;

internal class WPFWindowFocuser() : IWindowFocuser {

    public void TryToSetMainWindowFocus() {
        try {

            Application.Current.Dispatcher.Invoke(() => {
                Application.Current.MainWindow.Activate();
            });

        } catch {
            // ¯\_(ツ)_/¯
        }
    }

}
