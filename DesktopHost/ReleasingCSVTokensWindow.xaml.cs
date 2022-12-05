using ApplicationCore.Features.CLI;
using ApplicationCore.Infrastructure;
using System.Windows;
using System.Windows.Threading;

namespace DesktopHost;
/// <summary>
/// Interaction logic for ReleasingCSVTokensWindow.xaml
/// </summary>
public partial class ReleasingCSVTokensWindow : Window, IUIListener, IUIListener<CSVTokenProgressNotification> {
	
	public ReleasingCSVTokensWindow() {
		InitializeComponent();
	}

	public void Handle(CSVTokenProgressNotification notification) {
		Dispatcher.Invoke(() => ProgressMessageBox.AppendText(notification.Message + '\n'));
	}

}
