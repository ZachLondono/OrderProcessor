using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData;

namespace ApplicationCore.Features.Orders.OrderLoading.PickOrderSource;

internal class ChooseOrderProviderViewModel {

    public IEnumerable<SourceConfig> SourceConfigs { get; }

    public ChooseOrderProviderViewModel() {

        SourceConfigs = new List<SourceConfig>() {
            new() {
                Name = "Allmoxy Web",
                SourceType = OrderSourceType.AllmoxyWebXML,
                DialogTitle = "Enter Allmoxy Order Number",
                SourcePickerDialogType = typeof(GetAllmoxyOrderNumberDialog)
            },
            new() {
                Name = "Allmoxy File",
                SourceType = OrderSourceType.AllmoxyFileXML,
                DialogTitle = "Select Allmoxy Order File",
                SourcePickerDialogType = typeof(GetAllmoxyOrderFilePathDialog)
            },
            new() {
                Name = "Closet Pro",
                SourceType = OrderSourceType.ClosetProCSV,
                DialogTitle = "Select ClosetPro Order File",
                SourcePickerDialogType = typeof(GetClosetProOrderFilePathDialog)
            }
            //new() {
            //    Name = "Door Spreadsheet",
            //    SourceType = OrderSourceType.DoorOrder,
            //    DialogTitle = "Select Door Order Spreadsheet",
            //    SourcePickerDialogType = typeof(GetDoorOrderSpreadsheetPathDialog)
            //}
        };

    }

}
