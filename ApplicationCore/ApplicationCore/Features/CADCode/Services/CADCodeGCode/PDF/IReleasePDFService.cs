using ApplicationCore.Features.CADCode.Contracts.ProgramRelease;

namespace ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.PDF;

public interface IReleasePDFService {

    public IEnumerable<string> GeneratePDFs(ReleasedJob job, string outputDirectory);

}
