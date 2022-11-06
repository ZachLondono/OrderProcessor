using ApplicationCore.Features.CADCode.Services.Domain.PDF;

namespace ApplicationCore.Features.CADCode.Services;

internal interface IPDFConfigurationProvider {

    public PDFConfiguration GetConfiguration();

}