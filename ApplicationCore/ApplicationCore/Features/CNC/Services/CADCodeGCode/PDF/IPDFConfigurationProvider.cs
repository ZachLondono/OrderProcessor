using ApplicationCore.Features.CNC.Services.Domain.PDF;

namespace ApplicationCore.Features.CNC.Services.Services.CADCodeGCode.PDF;

public interface IPDFConfigurationProvider {
    public PDFConfiguration GetConfiguration();
}
