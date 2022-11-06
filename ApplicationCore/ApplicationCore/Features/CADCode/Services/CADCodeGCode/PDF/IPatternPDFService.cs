using ApplicationCore.Features.CADCode.Services.Domain.ProgramRelease;

namespace ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.PDF;

internal interface IReleasePDFService {

    public IEnumerable<string> GeneratePDFs(ReleasedJob job);

}
