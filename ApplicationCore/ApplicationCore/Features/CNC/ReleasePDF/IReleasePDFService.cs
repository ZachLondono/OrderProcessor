using ApplicationCore.Features.CNC.GCode.Contracts.ProgramRelease;

namespace ApplicationCore.Features.CNC.ReleasePDF;

public interface IReleasePDFService
{

    public IEnumerable<string> GeneratePDFs(ReleasedJob job, string outputDirectory);

}
