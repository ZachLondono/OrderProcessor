using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Shared;

namespace ApplicationCore.Features.CNC.ReleasePDF.Services;

internal interface IReleasePDFDecoratorFactory {

    public IEnumerable<IDocumentDecorator> GenerateDecorators(ReleasedJob job);

}
