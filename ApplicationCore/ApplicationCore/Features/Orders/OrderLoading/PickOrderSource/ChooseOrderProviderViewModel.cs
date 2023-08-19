using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadDoorSpreadsheetOrderData.DoorOrderModels;

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
                Name = "Closet Pro Web",
                SourceType = OrderSourceType.ClosetProWebCSV,
                DialogTitle = "Enter ClosetPro Order ID",
                SourcePickerDialogType = typeof(GetClosetProOrderIdDialog)
            },
            new() {
                Name = "Closet Pro File",
                SourceType = OrderSourceType.ClosetProFileCSV,
                DialogTitle = "Select ClosetPro Order File",
                SourcePickerDialogType = typeof(GetClosetProOrderFilePathDialog)
            },
            new() {
                Name = "Doweled DB Form",
                SourceType = OrderSourceType.DoweledDBOrderForm,
                DialogTitle = "Select Doweled DB Spreadsheet",
                SourcePickerDialogType = typeof(GetDoorOrderSpreadsheetPathDialog)
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
