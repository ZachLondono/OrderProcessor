namespace ApplicationCore.Features.Orders.Details.OrderExport;

public class ExportConfiguration {

    public string? OutputDirectory { get; set; }

    public bool FillDovetailOrder { get; set; }
    public string? DovetailTemplateFilePath { get; set; }

    public bool FillMDFDoorOrder { get; set; }
    public string? MDFDoorTemplateFilePath { get; set; }

    public bool GenerateEXT { get; set; }

}
