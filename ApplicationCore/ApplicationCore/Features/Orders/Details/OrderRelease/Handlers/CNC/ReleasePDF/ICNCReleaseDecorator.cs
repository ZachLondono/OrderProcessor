namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF;

public interface ICNCReleaseDecorator : IDocumentDecorator {

    public string ReportFilePath { get; set; }

}
