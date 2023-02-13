using ApplicationCore.Features.CNC.LabelDB.Contracts;
using ApplicationCore.Features.CNC.LabelDB.Services;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.CNC.LabelDB;

public class LoadJobFromLabelDB {

    public record Command(string LabelFilePath, string JobName) : ICommand<ExistingJob>;

    public class Handler : CommandHandler<Command, ExistingJob> {

        private readonly IExistingJobProvider _existingJobProvider;

        public Handler(IExistingJobProvider existingJobProvider) {
            _existingJobProvider = existingJobProvider;
        }

        public override async Task<Response<ExistingJob>> Handle(Command query) {

            var job = await _existingJobProvider.LoadExistingJobAsync(query.LabelFilePath, query.JobName);

            return new(job);

        }

    }

}