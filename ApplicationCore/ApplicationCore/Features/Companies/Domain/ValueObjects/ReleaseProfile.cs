namespace ApplicationCore.Features.Companies.Domain.ValueObjects;

public class ReleaseProfile {

    public required bool GenerateCutList { get; set; }
    public required string CutListOutputDirectory { get; set; }
    public required bool PrintCutList { get; set; }
    public required bool GeneratePackingList { get; set; }
    public required string PackingListOutputDirectory { get; set; }
    public required bool PrintPackingList { get; set; }
    public required bool GenerateInvoice { get; set; }
    public required string InvoiceOutputDirectory { get; set; }
    public required bool PrintInvoice { get; set; }
    public required bool GenerateBOL { get; set; }
    public required string BOLOutputDirectory { get; set; }
    public required bool PrintBOL { get; set; }
    public required string BOLTemplateFilePath { get; set; }
    public required bool PrintBoxLabels { get; set; }
    public required string BoxLabelsTemplateFilePath { get; set; }
    public required bool PrintOrderLabel { get; set; }
    public required string OrderLabelTemplateFilePath { get; set; }
    public required bool PrintADuiePyleLabel { get; set; }
    public required string ADuiePyleLabelTemplateFilePath { get; set; }
    public required bool GenerateCNCPrograms { get; set; }
    public required string CNCReportOutputDirectory { get; set; }
    public required bool FillDoorOrder { get; set; }
    public required bool GenerateDoorCNCPrograms { get; set; }
    public required string DoorOrderOutputDirectory { get; set; }
    public required string DoorOrderTemplateFilePath { get; set; }
    public required bool GenerateCabinetJobSummary { get; set; }
    public required string CabinetJobSummaryTemplateFilePath { get; set; }
    public required string CabinetJobSummaryOutputDirectory { get; set; }
    public required bool GenerateCabinetPackingList { get; set; }
    public required string CabinetPackingListTemplateFilePath { get; set; }
    public required string CabinetPackingListOutputDirectory { get; set; }

    // TODO: By default, the file paths will be set to directories in the installation folder
    public static ReleaseProfile Default => new() {
        GenerateCutList = false,
        CutListOutputDirectory = string.Empty,
        PrintCutList = false,
        GeneratePackingList = false,
        PackingListOutputDirectory = string.Empty,
        PrintPackingList = false,
        GenerateInvoice = false,
        InvoiceOutputDirectory = string.Empty,
        PrintInvoice = false,
        PrintBoxLabels = false,
        BoxLabelsTemplateFilePath = string.Empty,
        PrintOrderLabel = false,
        OrderLabelTemplateFilePath = string.Empty,
        PrintADuiePyleLabel = false,
        ADuiePyleLabelTemplateFilePath = string.Empty,
        BOLOutputDirectory = string.Empty,
        BOLTemplateFilePath = string.Empty,
        GenerateBOL = false,
        PrintBOL = false,
        GenerateCNCPrograms = false,
        CNCReportOutputDirectory = string.Empty,
        FillDoorOrder = false,
        GenerateDoorCNCPrograms = false,
        DoorOrderOutputDirectory = string.Empty,
        DoorOrderTemplateFilePath = string.Empty,
        GenerateCabinetJobSummary = false,
        CabinetJobSummaryTemplateFilePath = string.Empty,
        CabinetJobSummaryOutputDirectory = string.Empty,
        GenerateCabinetPackingList = false,
        CabinetPackingListTemplateFilePath = string.Empty,
        CabinetPackingListOutputDirectory = string.Empty
    };

}
