namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;

public record ParsingResult<T>(IEnumerable<string> Warnings, IEnumerable<string> Errors, T? Data);
