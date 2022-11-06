using ApplicationCore.Features.CADCode.Services.Domain.PDF;

namespace ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.PDF;

internal interface IPDFConfigurationProvider {
    public PDFConfiguration GetConfiguration();
}
