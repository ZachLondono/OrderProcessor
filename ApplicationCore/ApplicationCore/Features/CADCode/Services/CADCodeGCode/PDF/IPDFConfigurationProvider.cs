using ApplicationCore.Features.CADCode.Services.Domain.PDF;

namespace ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.PDF;

public interface IPDFConfigurationProvider {
    public PDFConfiguration GetConfiguration();
}
