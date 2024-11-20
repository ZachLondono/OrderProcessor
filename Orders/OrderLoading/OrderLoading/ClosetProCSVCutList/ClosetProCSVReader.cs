using OrderLoading.ClosetProCSVCutList.CSVModels;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace OrderLoading.ClosetProCSVCutList;

public class ClosetProCSVReader {

	public Action<string>? OnReadError { get; set; }

	public async Task<ClosetProOrderInfo> ReadCSVData(string csvData) {

		var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
			HasHeaderRecord = false,
			MissingFieldFound = null
		};

		OrderHeader orderInfo = new();
		List<Part> parts = [];
		List<PickPart> pickList = [];
		List<Accessory> accessories = [];

		using var reader = new StringReader(csvData);
		using var csv = new CsvReader(reader, config);

		bool wasOrderHeaderRead = false;

		CutListSection section = CutListSection.Parts;

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

            section = GetCurrentSection(section, csv.GetField(0), out bool isData);

            if (!isData) {
                continue;
            }

            try {

                switch (section) {

                    case CutListSection.Parts:

                        string partNum = csv.GetField(27) ?? "";

                        if (string.IsNullOrWhiteSpace(partNum) && csv.GetRecord<PartInfo>() is PartInfo info) {
                            parts.Last()?.InfoRecords.Add(info);
                        } else if (csv.GetRecord<Part>() is Part part) {
                            parts.Add(part);
                        }

                        break;

                    case CutListSection.PickList:

                        if (csv.GetRecord<PickPart>() is PickPart pickPart) {
                            pickList.Add(pickPart);
                        }

                        break;

                    case CutListSection.Accessories:

                        if (csv.GetRecord<Accessory>() is Accessory accessory) {
                            accessories.Add(accessory);
                        }

                        break;

                }

            } catch (Exception ex) {
                OnReadError?.Invoke($"Error parsing record starting with {csv.GetField(0)} - {ex.Message}");
            }

        }

        return new(orderInfo, parts, pickList, accessories);

	}

    private static CutListSection GetCurrentSection(CutListSection previosSection, string? field, out bool isData) {
        switch (field) {

            case "Cut List":
                isData = false;
                return CutListSection.Parts;

            case "Pick List":
                isData = false;
                return CutListSection.PickList;

            case "Accessories":
                isData = false;
                return CutListSection.Accessories;

            case "":
            case "Subtotal":
            case "Total":                  // End of cut list
            case "Applied Tax Rate":
            case "Base Shipping Costs":
            case "Grand Total":            // End of data
            case "Wall #":
            case "Part Type":
                isData = false;
                return previosSection;

            default:
                isData = true;
                return previosSection;

        };
    }

    public enum CutListSection {
		Parts,
		PickList,
		Accessories
	}

}
