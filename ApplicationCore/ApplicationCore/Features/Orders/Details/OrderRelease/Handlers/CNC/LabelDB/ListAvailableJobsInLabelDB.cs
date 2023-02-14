using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Services;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB;

public class ListAvailableJobsInLabelDB {

    public record Command(string LabelFilePath) : ICommand<IEnumerable<AvailableJob>>;

    public class Handler : CommandHandler<Command, IEnumerable<AvailableJob>> {

        private readonly IAvailableJobProvider _provider;

        public Handler(IAvailableJobProvider provider) {
            _provider = provider;
        }

        public override async Task<Response<IEnumerable<AvailableJob>>> Handle(Command command) {

            var jobs = await _provider.GetAvailableJobsFromLabelFileAsync(command.LabelFilePath);

            return new(jobs);


        }

    }

}