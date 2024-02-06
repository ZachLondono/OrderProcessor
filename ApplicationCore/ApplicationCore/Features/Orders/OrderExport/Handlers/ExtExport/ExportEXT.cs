using Domain.Orders;
using Domain.Orders.Entities;
using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Services;
using ApplicationCore.Shared.Services;
using Domain.Companies;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport;

public class ExportEXT {

    public record Command(Order Order, string JobName, string OutputDirectory) : ICommand<EXTGenerationResult>;

    public record EXTGenerationResult(string EXTFilePath, IEnumerable<PPProductManualParameters> RequiredManualParameters);

    public record PPProductManualParameters(string ProductName, int ProductSequenceNum, IDictionary<string, string> Parameters);

    public class Handler : CommandHandler<Command, EXTGenerationResult> {

        private readonly ILogger<Handler> _logger;
        private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;

        public Handler(ILogger<Handler> logger, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync) {
            _logger = logger;
            _getCustomerByIdAsync = getCustomerByIdAsync;
        }

        public override async Task<Response<EXTGenerationResult>> Handle(Command command) {

            List<PPProduct> products;
            try {

                products = command.Order
                                    .Products
                                    .Where(p => p is IPPProductContainer)
                                    .Cast<IPPProductContainer>()
                                    .SelectMany(c => c.GetPPProducts())
                                    .ToList();

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown while trying to generate Product Planner products for order");
                return new Error() {
                    Title = "Failed to generate EXT file",
                    Details = "An error occurred while trying to generate Product Planner parts for order"
                };

            }

            var jobName = command.JobName.Replace(".", "");

            var customer = await _getCustomerByIdAsync(command.Order.CustomerId);
            var customerName = customer?.Name ?? "";

            var job = new PPJob(jobName, command.Order.OrderDate, customerName, products);

            string defaultLevelName = command.Order.Name;
            if (defaultLevelName.Length > 60) defaultLevelName = defaultLevelName[..60];

            string filePath = "";

            try {

                var writer = new ExtWriter();
                new PPJobConverter(writer).ConvertOrder(job, defaultLevelName);
                filePath = writer.WriteFile(command.OutputDirectory, job.Name);

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown while trying to write EXT file");
                return new Error() {
                    Title = "Failed to generate EXT file",
                    Details = "An error occurred while trying to write PSI job to EXT file"
                };

            }

            var manualParams = products.Select(p => new PPProductManualParameters(p.Name, p.SequenceNum, p.ManualOverrideParameters));

            return new EXTGenerationResult(filePath, manualParams);

        }

    }

}