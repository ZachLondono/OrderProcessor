using ApplicationCore.Features.CNC.Contracts.ProgramRelease;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.LabelDB;

public class ExistingJobFromLabelFileHandler : QueryHandler<ExistingJobFromLabelFileQuery, ReleasedJob>
{

    private readonly IExistingJobProvider _existingJobProvider;

    public ExistingJobFromLabelFileHandler(IExistingJobProvider existingJobProvider)
    {
        _existingJobProvider = existingJobProvider;
    }

    public override async Task<Response<ReleasedJob>> Handle(ExistingJobFromLabelFileQuery query)
    {

        var job = await _existingJobProvider.LoadExistingJobAsync(query.LabelFilePath, query.JobName);

        return new(job);

    }

}