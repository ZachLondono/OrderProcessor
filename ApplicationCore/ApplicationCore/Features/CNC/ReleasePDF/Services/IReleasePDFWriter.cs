using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.Shared;

namespace ApplicationCore.Features.CNC.ReleasePDF.Services;

internal interface IReleasePDFWriter {

    public IEnumerable<IDocumentDecorator> GenerateDecorators(ReleasedJob job);

}
