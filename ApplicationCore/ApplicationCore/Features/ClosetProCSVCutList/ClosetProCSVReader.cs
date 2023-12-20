using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public class ClosetProCSVReader {

    public Action<string>? OnReadError { get; set; }

    private readonly ILogger<ClosetProCSVReader> _logger;

    public ClosetProCSVReader(ILogger<ClosetProCSVReader> logger) {
        _logger = logger;
    }

    public async Task<ClosetProOrderInfo> ReadCSVData(string csvData) {

        // TODO: make Async

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = false,
            MissingFieldFound = null
        };

        OrderHeader orderInfo = new();
        List<Part> parts = new();
        List<BuyOutPart> buyOutParts = new();
        List<PickPart> pickList = new();
        List<Accessory> accessories = new();

        using var reader = new StringReader(csvData);
        using var csv = new CsvReader(reader, config);

        bool wasOrderHeaderRead = false;
        bool isReadingCutList = false;
        bool isReadingPickList = false;
        bool isReadingAccessories = false;

        while (await csv.ReadAsync()) {

            if (!wasOrderHeaderRead) {

                orderInfo = new() {
                    Designer = csv.GetField(0) ?? "",
                    Customer = csv.GetField(1) ?? "",
                    DesignerCompany = csv.GetField(2) ?? "",
                    CustomerName = csv.GetField(3) ?? "",
                    OrderName = csv.GetField(4) ?? "",
                };

                wasOrderHeaderRead = true;
                continue;

            }

            switch (csv.GetField(0)) {

                case "":
                case "Subtotal":
                case "Total":           // End of cut list
                case "Applied Tax Rate":
                case "Base Shipping Costs":
                case "Grand Total":     // End of data
                case "Wall #":
                case "Part Type":
                    continue;

                case "Cut List":
                    isReadingCutList = true;
                    isReadingPickList = false;
                    isReadingAccessories = false;
                    continue;

                case "Pick List":
                    isReadingCutList = false;
                    isReadingPickList = true;
                    isReadingAccessories = false;
                    continue;

                case "Accessories":
                    isReadingCutList = false;
                    isReadingPickList = false;
                    isReadingAccessories = true;
                    continue;

            }

            try {

                if (isReadingCutList) {

                    string partType = csv.GetField(2) ?? "";
                    string partNum = csv.GetField(27) ?? "";

                    if (partNum == "" && csv.GetRecord<PartInfo>() is PartInfo partInfo) {
                        parts.Last()?.InfoRecords.Add(partInfo);
                    } else if ((partType == "Material" || partType == "Drawer" || partType == "Door" || partType == "Box" || partType == "Countertop") && csv.GetRecord<Part>() is Part part) {
                        parts.Add(part);
                    } else if (csv.GetRecord<BuyOutPart>() is BuyOutPart boPart) {
                        buyOutParts.Add(boPart);
                    }

                } else if (isReadingPickList && csv.GetRecord<PickPart>() is PickPart pickPart) {
                    pickList.Add(pickPart);
                } else if (isReadingAccessories && csv.GetRecord<Accessory>() is Accessory accessory) {
                    accessories.Add(accessory);
                }

            } catch (Exception ex) {
                _logger.LogError(ex, "Exception thrown while attempting to parse row starting with {FirstField}", csv.GetField(0));
                OnReadError?.Invoke($"Error parsing record starting with {csv.GetField(0)}");
            }

        }

        return new(orderInfo, parts, pickList, accessories, buyOutParts);

    }



}
