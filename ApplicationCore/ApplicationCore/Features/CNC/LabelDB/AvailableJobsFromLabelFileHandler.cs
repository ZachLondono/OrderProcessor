using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.LabelDB;

public class AvailableJobsFromLabelFileHandler : QueryHandler<AvailableJobsFromLabelFileQuery, IEnumerable<AvailableJob>>
{

    private readonly IAvailableJobProvider _provider;

    public AvailableJobsFromLabelFileHandler(IAvailableJobProvider provider)
    {
        _provider = provider;
    }

    public override async Task<Response<IEnumerable<AvailableJob>>> Handle(AvailableJobsFromLabelFileQuery query)
    {

        var jobs = await _provider.GetAvailableJobsFromLabelFileAsync(query.LabelFilePath);

        return new(jobs);


    }

}