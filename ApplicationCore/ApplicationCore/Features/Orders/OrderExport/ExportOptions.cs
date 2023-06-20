namespace ApplicationCore.Features.Orders.OrderExport;

public class ExportOptions {

    public string DovetailTemplateFilePath { get; set; } = string.Empty;

    public string MDFDoorTemplateFilePath { get; set; } = string.Empty;

    public string EXTOutputDirectory { get; set; } = string.Empty;

    public string CSVOutputDirectory { get; set; } = string.Empty;

}