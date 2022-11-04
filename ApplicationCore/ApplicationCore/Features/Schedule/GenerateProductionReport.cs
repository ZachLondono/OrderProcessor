using ApplicationCore.Features.ExcelTemplates.Contracts;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using ApplicationCore.Features.ExcelTemplates.Domain;

namespace ApplicationCore.Features.Schedule;

public class GenerateProductionReport {

    public record Command(IEnumerable<ScheduledOrder> Orders, string OutputDirectory, string ReportTitle, string TemplatePath, bool DoPrint) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly ILogger<Handler> _logger;
        private readonly IBus _bus;
        private readonly IDbConnectionFactory _factory;

        public Handler(ILogger<Handler> logger, IBus bus, IDbConnectionFactory factory) {
            _logger = logger;
            _bus = bus;
            _factory = factory;
        }
        public override async Task<Response> Handle(Command command) {

            // TODO: get all necessary order info for report

            object model = new {
                command.Orders
            };
            
            var config = new ClosedXMLTemplateConfiguration() {
                TemplateFilePath = command.TemplatePath
            };

            try { 

                var result = await _bus.Send(new FillTemplateRequest(model, command.OutputDirectory, command.ReportTitle, command.DoPrint, config));

                Response? response = null;
                result.Match(
                    inv => response = new(),
                    error => {
                        _logger.LogInformation("Error creating invoice {Error}", error);
                        response = new Response(error);
                    }
                );

                return response!;

            } catch (Exception e) {
                
                return new Response(new Error() {
                    Message = $"Error generating production report {e.Message}",
                });

            }

        }
    }

}