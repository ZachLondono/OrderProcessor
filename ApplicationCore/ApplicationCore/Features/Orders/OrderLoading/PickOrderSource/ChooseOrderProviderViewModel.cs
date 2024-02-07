using OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;
using OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;
using OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;
using OrderLoading.LoadDoorSpreadsheetOrderData.DoorOrderModels;
using OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;
using OrderLoading.LoadHafeleDBSpreadsheetOrderData;
using Microsoft.Extensions.Options;
using ApplicationCore.Features.Orders.OrderLoading;

namespace ApplicationCore.Features.Orders.OrderLoading.PickOrderSource;

public class ChooseOrderProviderViewModel {

    public IEnumerable<SourceConfig> SourceConfigs { get; }

    private readonly OrderProvidersConfiguration _configuration;

    public ChooseOrderProviderViewModel(IOptions<OrderProvidersConfiguration> configuration) {

        _configuration = configuration.Value;
        var sourceConfigs = new List<SourceConfig>();

        if (_configuration.AllmoxyWebXML) {
            sourceConfigs.Add(new() {
                Name = "Allmoxy Web",
                SourceType = OrderSourceType.AllmoxyWebXML,
                DialogTitle = "Enter Allmoxy Order Number",
                SourcePickerDialogType = typeof(GetAllmoxyOrderNumberDialog)
            });
        }

        if (_configuration.AllmoxyFileXML) {
            sourceConfigs.Add(new() {
                Name = "Allmoxy File",
                SourceType = OrderSourceType.AllmoxyFileXML,
                DialogTitle = "Select Allmoxy Order File",
                SourcePickerDialogType = typeof(GetAllmoxyOrderFilePathDialog)
            });
        }

        if (_configuration.ClosetProWebCSV) {
            sourceConfigs.Add(new() {
                Name = "Closet Pro Web",
                SourceType = OrderSourceType.ClosetProWebCSV,
                DialogTitle = "Enter ClosetPro Order ID",
                SourcePickerDialogType = typeof(GetClosetProOrderIdDialog)
            });
        }

        if (_configuration.ClosetProFileCSV) {
            sourceConfigs.Add(new() {
                Name = "Closet Pro File",
                SourceType = OrderSourceType.ClosetProFileCSV,
                DialogTitle = "Select ClosetPro Order File",
                SourcePickerDialogType = typeof(GetClosetProOrderFilePathDialog)
            });
        }

        if (_configuration.DoweledDBOrderForm) {
            sourceConfigs.Add(new() {
                Name = "Doweled DB Form",
                SourceType = OrderSourceType.DoweledDBOrderForm,
                DialogTitle = "Select Doweled DB Spreadsheet",
                SourcePickerDialogType = typeof(GetDoweledDBOrderSpreadsheetPathDialog)
            });
        }

        if (_configuration.ClosetOrderForm) {
            sourceConfigs.Add(new() {
                Name = "Closet Order Form",
                SourceType = OrderSourceType.ClosetOrderForm,
                DialogTitle = "Select Closet Order Spreadsheet",
                SourcePickerDialogType = typeof(GetClosetOrderSpreadsheetPathDialog)
            });
        }

        if (_configuration.HafeleDBOrderFor) {
            sourceConfigs.Add(new() {
                Name = "Hafele Dowel DB",
                SourceType = OrderSourceType.HafeleDBOrderForm,
                DialogTitle = "Select Hafele Order Spreadsheet",
                SourcePickerDialogType = typeof(GetHafeleDBOrderSpreadsheetPathDialog)
            });
        }

        if (_configuration.DoorOrder) {
            sourceConfigs.Add(new() {
                Name = "Door Spreadsheet",
                SourceType = OrderSourceType.DoorOrder,
                DialogTitle = "Select Door Order Spreadsheet",
                SourcePickerDialogType = typeof(GetDoorOrderSpreadsheetPathDialog)
            });
        }

        SourceConfigs = sourceConfigs;

    }

}
