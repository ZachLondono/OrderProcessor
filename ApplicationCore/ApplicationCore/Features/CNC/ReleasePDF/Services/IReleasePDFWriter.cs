using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Shared;

namespace ApplicationCore.Features.CNC.ReleasePDF.Services;

internal interface IReleasePDFWriter {

    public IEnumerable<IDocumentDecorator> GenerateDecorators(ReleasedJob job);

}
