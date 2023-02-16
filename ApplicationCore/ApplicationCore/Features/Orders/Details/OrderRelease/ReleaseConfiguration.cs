using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Contracts;

namespace ApplicationCore.Features.Orders.Details.OrderRelease;

public class ReleaseConfiguration {

    public string? OutputDirectory { get; set; }

    public bool GenerateInvoice { get; set; }

    public bool GeneratePackingList { get; set; }

    public string? CNCDataFilePath { get; set; }

    public IEnumerable<AvailableJob>? CNCJobs { get; set; }

    public bool GenerateCNCRelease { get; set; }

    public bool GenerateJobSummary { get; set; }

    public string? EmailRecipients { get; set; }

    public bool SendEmail { get; set; }

}
