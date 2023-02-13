using ApplicationCore.Features.CNC.ReleasePDF.Contracts;

namespace ApplicationCore.Features.CNC.ReleasePDF.Services;

public interface IReleasePDFWriter {

    public IEnumerable<string> GeneratePDFs(ReleasedJob job, string outputDirectory);

}
