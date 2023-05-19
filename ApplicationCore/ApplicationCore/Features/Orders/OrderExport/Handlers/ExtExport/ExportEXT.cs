using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Services;
using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport;

public class ExportEXT {
    
    public record Command(Order Order, string JobName, string OutputDirectory) : ICommand<string>;

    public class Handler : CommandHandler<Command, string> {

        private readonly IFileReader _fileReader;
        private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    
        public Handler(IFileReader fileReader, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync) {
            _fileReader = fileReader;
            _getCustomerByIdAsync = getCustomerByIdAsync;
        }
    
        public override async Task<Response<string>> Handle(Command command) {
    
            var products = command.Order.Products
                                .Where(p => p is IPPProductContainer)
                                .Cast<IPPProductContainer>()
                                .SelectMany(c => c.GetPPProducts())
                                .ToList();
    
            var jobName = command.JobName.Replace(".", "");
            if (jobName.Length > 30) jobName = jobName[..30];
    
            var customer = await _getCustomerByIdAsync(command.Order.CustomerId);
            var customerName = customer?.Name ?? "";
    
            var job = new PPJob(jobName, command.Order.OrderDate, customerName, products);
    
            string filePath = Path.Combine(command.OutputDirectory, $"{_fileReader.RemoveInvalidPathCharacters(jobName)}.ext");
    
            var writer = new ExtWriter();
    
            new PPJobConverter(writer).ConvertOrder(job);
    
            writer.WriteFile(filePath);
    
            string errors = "";
            int index = 0;
            foreach (var product in products) {
                index++;
                foreach (var (key, value) in product.ManualOverrideParameters) {
    
                    errors += $"[cab:{index}] {key} ==>> {value}<br>";
    
                }
            }
    
            return new(filePath);
    
        }

    }

}