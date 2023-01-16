using ApplicationCore.Features.Shared;
using System.Windows;

namespace DesktopHost.Dialogs;

internal class WPFMessageBox : IMessageBoxService {

    public void OpenDialog(string text, string caption) {
        MessageBox.Show(text, caption, MessageBoxButton.OK);
    }

    public OKCancelResult OpenDialogOKCancel(string text, string caption) {
        var result = MessageBox.Show(text, caption, MessageBoxButton.OKCancel);

        return result switch {
            MessageBoxResult.OK => OKCancelResult.OK,
            MessageBoxResult.Cancel => OKCancelResult.Cancel,
            _ => OKCancelResult.Unknown,
        };
    }

    public YesNoResult OpenDialogYesNo(string text, string caption) {
        var result = MessageBox.Show(text, caption, MessageBoxButton.YesNo);

        return result switch {
            MessageBoxResult.Yes => YesNoResult.Yes,
            MessageBoxResult.No => YesNoResult.No,
            _ => YesNoResult.Unknown,
        };
    }
}
