namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;

public class PDFGenerationResult {

    public IEnumerable<string> FilePaths { get; set; } = Enumerable.Empty<string>();

}
