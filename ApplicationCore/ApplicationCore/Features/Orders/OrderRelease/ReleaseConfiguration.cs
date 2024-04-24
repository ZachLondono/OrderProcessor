namespace ApplicationCore.Features.Orders.OrderRelease;

public class ReleaseConfiguration {

    private bool _sendReleaseEmail = false;
    private bool _previewReleaseEmail = false;
    private bool _includeCheckBoxesInPackingList = false;
    private bool _includeSignatureFieldInPackingList = false;
    private bool _includeDovetailDBPackingList = false;

    public List<string> AdditionalFilePaths { get; set; } = new();
    public bool AttachAdditionalFiles { get; set; } = false;
    public List<string> CNCDataFilePaths { get; set; } = new();
    public bool GenerateCNCRelease { get; set; }
    public bool CopyCNCReportToWorkingDirectory { get; set; }
    public bool GenerateCNCGCode { get; set; }

    public bool GeneratePackingList { get; set; }
    public bool IncludeCheckBoxesInPackingList {
        get => GeneratePackingList && _includeCheckBoxesInPackingList;
        set => _includeCheckBoxesInPackingList = value;
    }
    public bool IncludeSignatureFieldInPackingList {
        get => GeneratePackingList && _includeSignatureFieldInPackingList;
        set => _includeSignatureFieldInPackingList = value;
    }
    public bool IncludeDovetailDBPackingList {
        get => GeneratePackingList && _includeDovetailDBPackingList;
        set => _includeDovetailDBPackingList = value;
    }

    public bool GenerateHardwareList { get; set; }

    public bool GenerateJobSummary { get; set; }
    public bool IncludeProductTablesInSummary { get; set; }
    public bool IncludeInvoiceInRelease { get; set; }
    public bool Generate5PieceCutList { get; set; }
    public bool GenerateDoweledDrawerBoxCutList { get; set; }
    public string? ReleaseEmailRecipients { get; set; }

    public bool SendReleaseEmail {
        get => AreMainPDFOptionsEnabled && _sendReleaseEmail;
        set => _sendReleaseEmail = value;
    }
    public bool PreviewReleaseEmail {
        get => SendReleaseEmail && _previewReleaseEmail;
        set => _previewReleaseEmail = value;
    }

    public bool IncludeMaterialSummaryInEmailBody { get; set; }
    public string? ReleaseFileName { get; set; }
    public string? ReleaseOutputDirectory { get; set; }

    public bool GenerateInvoice { get; set; }
    public string? InvoiceFileName { get; set; }
    public string? InvoiceOutputDirectory { get; set; }
    public string? InvoiceEmailRecipients { get; set; }
    public bool SendInvoiceEmail { get; set; }
    public bool PreviewInvoiceEmail { get; set; }

    public bool AreMainPDFOptionsEnabled => (GenerateJobSummary || GeneratePackingList || GenerateCNCRelease || GenerateCNCGCode || GenerateDoweledDrawerBoxCutList || Generate5PieceCutList || IncludeDovetailDBPackingList || GenerateHardwareList);

}
