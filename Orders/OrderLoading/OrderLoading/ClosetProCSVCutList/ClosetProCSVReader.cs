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

            section = GetCurrentSection(section, csv.GetField(0));

            try {

                switch (section) {

                    case CutListSection.Parts:

                        string partNum = csv.GetField(27) ?? "";
                        if (csv.GetRecord<Part>() is Part part) {

                            if (string.IsNullOrWhiteSpace(partNum)) {
                                parts.Last()?.InfoRecords.Add(part);
                            } else {
                                parts.Add(part);
                            }

                            break;

                        }

                        throw new InvalidDataException("Failed to Parse Part");

                    case CutListSection.PickList:

                        if (csv.GetRecord<PickPart>() is PickPart pickPart) {
                            pickList.Add(pickPart);
                            break;
                        }

                        throw new InvalidDataException("Failed to Parse Pick List Part");

                    case CutListSection.Accessories:

                        if (csv.GetRecord<Accessory>() is Accessory accessory) {
                            accessories.Add(accessory);
                            break;
                        }

                        throw new InvalidDataException("Failed to Parse Accessory");

                }

            } catch (Exception ex) {
                OnReadError?.Invoke($"Error parsing record starting with {csv.GetField(0)} - {ex.Message}");
            }

        }

        return new(orderInfo, parts, pickList, accessories);

	}

    private static CutListSection GetCurrentSection(CutListSection previosSection, string? field) {
        return field switch {

            "" or
            "Subtotal" or
            "Total" or                  // End of cut list
            "Applied Tax Rate" or
            "Base Shipping Costs" or
            "Grand Total" or            // End of data
            "Wall #" or
            "Part Type" => previosSection,

            "Cut List" => CutListSection.Parts,

            "Pick List" => CutListSection.PickList,

            "Accessories" => CutListSection.Accessories,

            _ => throw new InvalidDataException("Unexpected CSV cut list format")

        };
    }

    public enum CutListSection {
		Parts,
		PickList,
		Accessories,
		Skip
	}

}
