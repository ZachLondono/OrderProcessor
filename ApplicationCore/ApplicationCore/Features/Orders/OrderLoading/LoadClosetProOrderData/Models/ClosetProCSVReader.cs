using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;

public class ClosetProCSVReader {
    
    public Action<string>? OnReadError { get; set; }

    private readonly ILogger<ClosetProCSVReader> _logger;

    public ClosetProCSVReader(ILogger<ClosetProCSVReader> logger) {
        _logger = logger;
    }

    public ClosetProOrderInfo ReadCSVFile(string filePath) {

        // TODO: make Async

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = false,
        };
        
        OrderHeader orderInfo = new();
        List<Part> parts = new();
        List<BuyOutPart> buyOutParts = new();
        List<PickPart> pickList = new();
        List<Accessory> accessories = new();
        
        using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, config);
        
        bool wasOrderHeaderRead = false;
        bool isReadingCutList = false;
        bool isReadingPickList = false;
        bool isReadingAccessories = false;
        
        while (csv.Read()) {
        
            if (!wasOrderHeaderRead) {
            
                orderInfo = new() {
                    Designer = csv.GetField(0) ?? "",
                    B = csv.GetField(0) ?? "",
                    C = csv.GetField(0) ?? "",
                    CustomerName = csv.GetField(0) ?? "",
                    OrderName = csv.GetField(0) ?? "",
                };
            
                wasOrderHeaderRead = true;
                continue;
                
            }
            
            switch (csv.GetField(0)) {
            
                case "":
                case "Subtotal":
                case "Total":           // End of cut list
                case "Applied Tax Rate":
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
                    } else if ((partType == "Material" || partType == "Drawer" || partType == "Door") && csv.GetRecord<Part>() is Part part) {
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

        return new(orderInfo, parts, pickList, accessories);

    }



}
