using Outlook = Microsoft.Office.Interop.Outlook;

namespace ApplicationCore.Features.ClosetOrders.ClosetOrderImport;

public class OutlookWrapper {

    public bool IsInitialized => _app is not null;
    private Outlook.Application? _app = null;

    public void Initialize() {

        _app = OutlookApplicationRetriever.GetApplication();

    }

    public Outlook.MailItem? GetCurrentlySelectedMailItem() {

        if (_app is null) return null;

        Outlook.Explorer explorer = _app.ActiveExplorer();
        Outlook.Selection selection = explorer.Selection;

        if (selection.Count <= 0) {
            return null;
        }

        object selectedItem = selection[1];   // Index is one-based.

        if (selectedItem is not Outlook.MailItem mailItem) {
            return null;
        }

        return mailItem;

    }

}
