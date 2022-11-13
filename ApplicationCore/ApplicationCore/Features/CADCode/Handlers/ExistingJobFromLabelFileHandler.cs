using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Contracts.ProgramRelease;
using ApplicationCore.Features.CADCode.Services;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Handlers;

public class ExistingJobFromLabelFileHandler : QueryHandler<ExistingJobFromLabelFileQuery, ReleasedJob> {

    private readonly IExistingJobProvider _existingJobProvider;

    public ExistingJobFromLabelFileHandler(IExistingJobProvider existingJobProvider) {
		_existingJobProvider = existingJobProvider;
	}

	public override async Task<Response<ReleasedJob>> Handle(ExistingJobFromLabelFileQuery query) {

        var job = await _existingJobProvider.LoadExistingJobAsync(query.LabelFilePath, query.JobName);

        return new(job);

	}

}