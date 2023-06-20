namespace ApplicationCore.Features.Orders.OrderExport;

public class ExportConfiguration {

    public string? OutputDirectory { get; set; }

    public bool FillDovetailOrder { get; set; }

    public bool FillMDFDoorOrder { get; set; }

    public string ExtJobName { get; set; } = string.Empty;

    public bool GenerateEXT { get; set; }

    public string CsvJobName { get; set; } = string.Empty;

    public bool GenerateCSV { get; set; }

}
