using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Loader.Providers.DoorOrderModels;

namespace ApplicationCore.Features.Orders.Loader.Providers.Dialog;

internal class ChooseOrderProviderViewModel {

    public IEnumerable<SourceConfig> SourceConfigs { get; }

    public ChooseOrderProviderViewModel() {

        SourceConfigs = new List<SourceConfig>() {
            new() {
                Name = "Allmoxy",
                SourceType = OrderSourceType.AllmoxyXML,
                DialogTitle = "Enter Allmoxy Order Number",
                SourcePickerDialogType = typeof(GetAllmoxyOrderNumberDialog)
            },
             new() {
                Name = "Door Spreadsheet",
                SourceType = OrderSourceType.DoorOrder,
                DialogTitle = "Enter Path to Door Order Spreadsheet",
                SourcePickerDialogType = typeof(GetDoorOrderSpreadsheetPathDialog)
            } 
        };

    }

}
