using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Services;

public interface IReleasePDFWriter {

    public IEnumerable<string> GeneratePDFs(ReleasedJob job, string outputDirectory);

}
