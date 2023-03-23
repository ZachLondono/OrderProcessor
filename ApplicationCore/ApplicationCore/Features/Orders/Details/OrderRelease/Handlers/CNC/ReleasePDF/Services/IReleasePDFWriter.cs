using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Services;

internal interface IReleasePDFWriter {

    public IEnumerable<IDocumentDecorator> GenerateDecorators(ReleasedJob job);

}
