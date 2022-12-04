using ApplicationCore.Features.CNC.GCode.Contracts.ProgramRelease;

namespace ApplicationCore.Features.CNC.ReleasePDF.Services;

public interface IReleasePDFWriter {

    public IEnumerable<string> GeneratePDFs(ReleasedJob job, string outputDirectory);

}
