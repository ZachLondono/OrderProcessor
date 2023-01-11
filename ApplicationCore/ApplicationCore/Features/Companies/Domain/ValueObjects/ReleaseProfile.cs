namespace ApplicationCore.Features.Companies.Domain.ValueObjects;

public class ReleaseProfile {

    public bool GenerateCutList { get; set; }
    public string CutListOutputDirectory { get; set; } = string.Empty;
    public bool PrintCutList { get; set; }
    public string CutListTemplatePath { get; set; } = string.Empty;// TODO rename to ...FilePath

    public bool GeneratePackingList { get; set; }
    public string PackingListOutputDirectory { get; set; } = string.Empty;
    public bool PrintPackingList { get; set; }
    public string PackingListTemplatePath { get; set; } = string.Empty; // TODO rename to ...FilePath

    public bool GenerateInvoice { get; set; }
    public string InvoiceOutputDirectory { get; set; } = string.Empty;
    public bool PrintInvoice { get; set; }
    public string InvoiceTemplatePath { get; set; } = string.Empty;// TODO rename to ...FilePath

    public bool GenerateBOL { get; set; }
    public string BOLOutputDirectory { get; set; } = string.Empty;
    public bool PrintBOL { get; set; }
    public string BOLTemplateFilePath { get; set; } = string.Empty;

    public bool PrintBoxLabels { get; set; }
    public string BoxLabelsTemplateFilePath { get; set; } = string.Empty;

    public bool PrintOrderLabel { get; set; }
    public string OrderLabelTemplateFilePath { get; set; } = string.Empty;

    public bool PrintADuiePyleLabel { get; set; }
    public string ADuiePyleLabelTemplateFilePath { get; set; } = string.Empty;

    public bool GenerateCNCPrograms { get; set; }
    public string CNCReportOutputDirectory { get; set; } = string.Empty;

    public bool FillDoorOrder { get; set; }
    public bool GenerateDoorCNCPrograms { get; set; }
    public string DoorOrderOutputDirectory { get; set; } = string.Empty;
    public string DoorOrderTemplateFilePath { get; set; } = string.Empty;

    //public bool WriteToGoogleSheets { get; set; }
    //public string GoogleSheetCode { get; set; } = string.Empty;

    // TODO: By default, the file paths will be set to directories in the installation folder
    public static ReleaseProfile Default => new() {
        GenerateCutList = false,
        CutListOutputDirectory = string.Empty,
        PrintCutList = false,
        CutListTemplatePath = string.Empty,
        GeneratePackingList = false,
        PackingListOutputDirectory = string.Empty,
        PackingListTemplatePath = string.Empty,
        PrintPackingList = false,
        GenerateInvoice = false,
        InvoiceOutputDirectory = string.Empty,
        InvoiceTemplatePath = string.Empty,
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
        DoorOrderTemplateFilePath = string.Empty
    };

}
