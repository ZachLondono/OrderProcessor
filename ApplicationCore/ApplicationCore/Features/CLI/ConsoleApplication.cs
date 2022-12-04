using ApplicationCore.Features.CNC.CSV;
using ApplicationCore.Features.Orders.Loader;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using CommandLine;

namespace ApplicationCore.Features.CLI;

public class ConsoleApplication {

    private readonly IBus _bus;
    private readonly IMessageBoxService _messageBoxService;

    public ConsoleApplication(IBus bus, IMessageBoxService messageBoxService) {
        _bus = bus;
        _messageBoxService = messageBoxService;
    }

    public async Task Run(string[] args) {

		await Parser.Default
                    .ParseArguments<ConsoleApplicationOption>(args)
                    .WithNotParsed(errors => {
                        
                        string errorMessage = "";
                        foreach (var error in errors) {
                            errorMessage += error + "\n";
                        }
                        _messageBoxService.OpenDialog(errorMessage, "Error");

                    }).WithParsedAsync(async option => {
                        
                        if (!string.IsNullOrWhiteSpace(option.CSVTokenFilePath)) {

							var result = await _bus.Send(new ReleaseCADCodeCSVBatchCommand(option.CSVTokenFilePath, @"C:\Users\Zachary Londono\Desktop\ExampleConfiguration\cutlists"));
                            result.Match(
                                success => {
                                    _messageBoxService.OpenDialog("Files created", "Success");
								},
                                error => {
									_messageBoxService.OpenDialog(error.Details, error.Title);
								}
                            );

						} else { 

                            var provider = ParseProviderType(option.Provider);
                            if (provider is not OrderSourceType.Unknown) {
                                var result = await _bus.Send(new LoadOrderCommand.Command(provider, option.Source));
                                result.Match(
                                order => _messageBoxService.OpenDialog($"New order loaded\n{order.Name}\n{order.Id}", "New Order"),
                                error => _messageBoxService.OpenDialog($"Error loading order\n{error.Details}", "Error")
                                );
                            } else {
                                _messageBoxService.OpenDialog($"Unknown order provider '{option.Provider}'", "Unknown provider");
                            }

						}

					});

	}

    private static OrderSourceType ParseProviderType(string provider) => provider switch {
        "allmoxy" => OrderSourceType.AllmoxyXML,
        "hafele" => OrderSourceType.HafeleExcel,
        "richelieu" => OrderSourceType.RichelieuXML,
        "ot" => OrderSourceType.OTExcel,
        _ => OrderSourceType.Unknown
    };

}