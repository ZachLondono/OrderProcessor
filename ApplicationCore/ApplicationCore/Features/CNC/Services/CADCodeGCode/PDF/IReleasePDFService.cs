using ApplicationCore.Features.CNC.Contracts.ProgramRelease;

namespace ApplicationCore.Features.CNC.Services.Services.CADCodeGCode.PDF;

public interface IReleasePDFService {

    public IEnumerable<string> GeneratePDFs(ReleasedJob job, string outputDirectory);

}
