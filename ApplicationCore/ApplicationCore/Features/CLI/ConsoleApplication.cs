using ApplicationCore.Features.CNC;
using ApplicationCore.Features.CNC.CSV;
using ApplicationCore.Features.CNC.GCode;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.ReleasePDF;
using ApplicationCore.Features.CNC.ReleasePDF.Contracts;
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

                            await GenerateReleaseFromTokenCSV(option.CSVTokenFilePath);

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

    private async Task GenerateReleaseFromTokenCSV(string filepath) {

		var result = await _bus.Send(new ReadTokensFromCSVFile.Command(filepath));
		result.Match(
			async result => {

                var parser = new CSVTokensParser();
                var batch = parser.ParseTokens(result);
				await GenerateGCodeForBatch(batch);

			},
            error => _messageBoxService.OpenDialog(error.Details, error.Title)
		);

	}

    private async Task GenerateGCodeForBatch(CNCBatch batch) {

        var response = await _bus.Send(new GenerateGCode.Command(batch));
		response.Match(
            result => _ = GeneratePDFRelease(result),
            error =>  _messageBoxService.OpenDialog(error.Details, error.Title)
        );

    }

    private async Task GeneratePDFRelease(ReleasedJob job) {

		var pdfResponse = await _bus.Send(new GenerateCNCReleasePDF.Command(job, @"C:\Users\Zachary Londono\Desktop\ExampleConfiguration\cutlists"));
		pdfResponse.Match(
			pdfResult => {
				string message = "";
				foreach (var file in pdfResult.FilePaths) {
					message += file + "\n";
				}
				_messageBoxService.OpenDialog(message, "Generated Files");
			},
			error => _messageBoxService.OpenDialog(error.Details, error.Title)
		);

	}


    private static OrderSourceType ParseProviderType(string provider) => provider switch {
        "allmoxy" => OrderSourceType.AllmoxyXML,
        "hafele" => OrderSourceType.HafeleExcel,
        "richelieu" => OrderSourceType.RichelieuXML,
        "ot" => OrderSourceType.OTExcel,
        _ => OrderSourceType.Unknown
    };

}