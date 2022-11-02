using ApplicationCore.Shared;
using Microsoft.Win32;

namespace DesktopHost.Dialogs;

public class WPFDialogFilePicker : IFilePicker {

    public bool TryPickFile(string title, string directory, FilePickerFilter filter, out string fileName) {

        fileName = string.Empty;
        var dialog = new OpenFileDialog();
        dialog.InitialDirectory = directory;
        dialog.Multiselect = false;
        dialog.Title = title;
        dialog.Filter = filter.ToFilterString();
        
        bool? result = dialog.ShowDialog();
        if (result is null) return false;
        bool wasPicked = (bool) result;
        if (!wasPicked) return false;

        fileName = dialog.FileName;
        return true;

    }

}