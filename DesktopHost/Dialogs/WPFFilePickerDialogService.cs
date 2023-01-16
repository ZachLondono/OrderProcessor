using ApplicationCore.Features.Shared;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace DesktopHost.Dialogs;

public class WPFDialogFilePicker : IFilePicker {

    public async Task<bool> PickFileAsync(string title, string directory, FilePickerFilter filter, Action<string> onFilePicked) {

        // The file picker must be run in a seperate thread than the ui, otherwise it will crash the WPF application if the user takes too long to pick a file

        string? result = await Task.Run(() => {

            var dialog = new OpenFileDialog {
                InitialDirectory = directory,
                Multiselect = false,
                Title = title,
                Filter = filter.ToFilterString()
            };

            bool? result = dialog.ShowDialog();

            if (result is not null && (bool)result) return dialog.FileName;

            return null;

        });

        if (result is null) return false;

        onFilePicked(result);
        return true;

    }

}