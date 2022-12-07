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
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.CLI;

public class ConsoleApplication {

    private readonly IBus _bus;
    private readonly IUIBus _uiBus;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ILogger<ConsoleApplication> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public ConsoleApplication(IBus bus, IUIBus uiBus, IMessageBoxService messageBoxService, ILogger<ConsoleApplication> logger, ILoggerFactory loggerFactory) {
        _bus = bus;
        _uiBus = uiBus;
        _messageBoxService = messageBoxService;
        _logger = logger;
		_loggerFactory = loggerFactory;
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

                            _logger.LogInformation("Generating release for tokens at {FilePath}", option.CSVTokenFilePath);

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

		_uiBus.Publish(new CSVTokenProgressNotification($"Reading tokens from file: '{filepath}'"));

		_logger.LogInformation("Reading tokens from csv file");

		var result = await _bus.Send(new ReadTokensFromCSVFile.Command(filepath));

        List<CNCBatch> batches = new();

		result.Match(
			result => {

                _logger.LogInformation("Converting csv tokens to CNC operations");
				_uiBus.Publish(new CSVTokenProgressNotification("Converting csv tokens to CNC operations"));

				var parser = new CSVTokensParser(_loggerFactory.CreateLogger<CSVTokensParser>());
                var parsedBatches = parser.ParseTokens(result).ToList();
                batches.AddRange(parsedBatches);

			},
            error => {
				_logger.LogError("Error reading batches from csv tokens {Error}", error);
                _messageBoxService.OpenDialog(error.Details, error.Title);
            }
		);


		_logger.LogInformation("Found {BatchCount} batches in csv tokens", batches.Count);
		_uiBus.Publish(new CSVTokenProgressNotification($"Found {batches.Count} batches in csv tokens"));

		foreach (var batch in batches) {
			await GenerateGCodeForBatch(batch);
		}

	}

    private async Task GenerateGCodeForBatch(CNCBatch batch) {

		_logger.LogInformation("Generating gcode for batch {BatchName}", batch.Name);

        ReleasedJob? job = null;

        var response = await _bus.Send(new GenerateGCode.Command(batch));
        response.Match(
            result => job = new GCodeToReleasedJobConverter().ConvertResult(result, batch.Name, batch.Parts),
            error => {
                _logger.LogError("Error generating GCode for batch {BatchName} {Error}", batch.Name, error);
                _messageBoxService.OpenDialog(error.Details, error.Title);
            }
        );

        if (job is not null) {
            await GeneratePDFRelease(job);
        } else {
            _logger.LogError("GenerateGCode command returned null or an error");
        }

    }

    private async Task GeneratePDFRelease(ReleasedJob job) {

		_uiBus.Publish(new CSVTokenProgressNotification($"Generating PDF for job '{job.JobName}'"));
		_logger.LogInformation("Generating PDF for job {JobName}", job.JobName);

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