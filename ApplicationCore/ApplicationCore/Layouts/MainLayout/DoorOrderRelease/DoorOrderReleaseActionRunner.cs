using ApplicationCore.Features.OpenDoorOrders;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.CNC;
using ApplicationCore.Shared.CNC;
using ApplicationCore.Shared.CNC.Job;
using ApplicationCore.Shared.CNC.ReleasePDF;
using ApplicationCore.Shared.Components.ProgressModal;
using ApplicationCore.Shared.Excel;
using ApplicationCore.Shared.Services;
using CADCodeProxy.CSV;
using CADCodeProxy.Machining;
using Microsoft.Office.Interop.Excel;
using QuestPDF.Fluent;
using System.Text.Json;
using UglyToad.PdfPig.Writer;
using static ApplicationCore.Layouts.MainLayout.DoorOrderRelease.NamedPipeServer;
using Action = System.Action;

namespace ApplicationCore.Layouts.MainLayout.DoorOrderRelease;

public class DoorOrderReleaseActionRunner : IActionRunner {

    public Action? ShowProgressBar { get; set; }
    public Action? HideProgressBar { get; set; }
    public Action<int>? SetProgressBarValue { get; set; }
    public Action<ProgressLogMessage>? PublishProgressMessage { get; set; }

    public DoorOrder? DoorOrder { get; set; }

    private readonly CNCPartGCodeGenerator _generator;
	private readonly CNCReleaseDecoratorFactory _releaseDecoratorFactory;
    private readonly IFileReader _fileReader;

	public DoorOrderReleaseActionRunner(CNCPartGCodeGenerator generator, CNCReleaseDecoratorFactory releaseDecoratorFactory, IFileReader fileReader) {
        _generator = generator;
		_releaseDecoratorFactory = releaseDecoratorFactory;
        _fileReader = fileReader;

        _generator.ShowProgressBar += () => ShowProgressBar?.Invoke();
        _generator.HideProgressBar += () => HideProgressBar?.Invoke();
        _generator.OnProgressReport += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, msg));
        _generator.OnError += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, msg));;
        _generator.SetProgressBarValue += (prog) => SetProgressBarValue?.Invoke(prog);

	}

    public async Task Run() {

        if (DoorOrder is null) return;

        await ReleaseDoorOrder(_generator, DoorOrder);

    }

    public async Task ReleaseDoorOrder(CNCPartGCodeGenerator generator, DoorOrder doorOrder) {

        if (!File.Exists(doorOrder.OrderFile) || Path.GetExtension(doorOrder.OrderFile) != ".xlsm") {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Invalid door order file"));
            return;
        }

        using var app = new ExcelApplicationWrapper() {
            Visible = false,
            DisplayAlerts = false
        };
        using var workbooks = app.Workbooks;
        using var workbook = workbooks.Open(doorOrder.OrderFile, readOnly: true);
        using var worksheets = workbook.Worksheets;

        var batches = await Task.Run(() => GenerateBatchesFromDoorOrder(app, worksheets, doorOrder));
        Document document = await CreateCutListDocumentForBatches(generator, doorOrder, batches);

        var tmpReleasePDFFilePath = GeneratePDFFromWorkbook(workbook, worksheets);
        string mergedFilePath = await MergeReleasePDF(doorOrder, tmpReleasePDFFilePath, document);

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, mergedFilePath));

    }

    private async Task<string> MergeReleasePDF(DoorOrder doorOrder, string? tmpReleasePDFFilePath, Document document) {

        var fileComponents = new List<byte[]>();

        if (tmpReleasePDFFilePath is not null) {
            var mdfReleasePagesData = await File.ReadAllBytesAsync(tmpReleasePDFFilePath);
            fileComponents.Add(mdfReleasePagesData);
        }

        var mergedDocument = await Task.Run(() => {

            var pdfData = document.GeneratePdf();
            fileComponents.Add(pdfData);

            return PdfMerger.Merge(fileComponents);

        });

        var mergedFilePath = _fileReader.GetAvailableFileName(@"C:\Users\Zachary Londono\Desktop\TestOutput", $"{doorOrder.OrderNumber} CUTLIST", ".pdf");
        await File.WriteAllBytesAsync(mergedFilePath, mergedDocument);

        return mergedFilePath;

    }

    private Batch[] GenerateBatchesFromDoorOrder(ExcelApplicationWrapper app, WorksheetsWrapper worksheets, DoorOrder doorOrder) {

        using var dataSheet = worksheets["MDF Door Data"];

        var exportDirectory = dataSheet.Range["ExportFile"].Value2;

        var tokenFile = Path.Combine(exportDirectory, $"{doorOrder.OrderNumber} - DoorTokens.csv");

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Generating CSV Token File"));
        var fileName = Path.GetFileName(doorOrder.OrderFile);

        var server = new NamedPipeServer();
        server.MessageReceived += ProcessMessage;
        var serverTask = Task.Run(server.Start);

        ShowProgressBar?.Invoke();
        var macroTask = Task.Run(() => app.RunMacro(fileName, "SilentDoorProcessing"));
        macroTask.Wait();
        HideProgressBar?.Invoke();

        server.Stop();

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Reading CSV Token File"));
        var batches = new CSVTokenReader().ReadBatchCSV(tokenFile);

        return batches;

    }

    private async Task<Document> CreateCutListDocumentForBatches(CNCPartGCodeGenerator generator, DoorOrder doorOrder, Batch[] batches) {

        List<ICNCReleaseDecorator> decorators = [];

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Generating GCode For Doors"));

        foreach (var batch in batches) {

            var job = await generator.GenerateGCode(batch, doorOrder.Customer, doorOrder.Vendor, DateTime.Today, DateTime.Today);

            if (job is null) {
                continue;
            }

            var decorator = _releaseDecoratorFactory.Create(job);
            decorators.Add(decorator);

            await WriteGCodeResultFile(job);

        }

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Creating CNC Release Document"));

        var document = await Task.Run(() => {
            return Document.Create(doc => {

                foreach (var decorator in decorators) {
                    decorator.Decorate(doc);
                }

            });
        });

        return document;

    }

    private void ProcessMessage(PipeMessage message) {

		switch (message.Type) {
			case "info":
				PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, $"{message.MessageA} - {message.MessageB}"));
				break;
			case "warning":
				PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"WARNING {message.MessageA} - {message.MessageB}"));
				break;
			case "error":
				PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"{message.MessageA} - {message.MessageB}"));
				break;
			case "progress":
				PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, $"{message.MessageA}"));
				if (int.TryParse(message.MessageB, out int percentComplete)) {
					SetProgressBarValue?.Invoke(percentComplete);
				}
				break;
			default:
				PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, $"{message.Type}|{message.MessageA}|{message.MessageB}"));
				break;
		}

	}

	private async Task WriteGCodeResultFile(ReleasedJob job) {
		var jobFileName = _fileReader.GetAvailableFileName(@"C:\Users\Zachary Londono\Desktop\TestOutput", $"{job.JobName} CNC RESULT", ".json");
		using FileStream fileStream = File.Create(jobFileName);
		await JsonSerializer.SerializeAsync(fileStream, job, new JsonSerializerOptions() {
			WriteIndented = true
		});
		await fileStream.DisposeAsync();
	}

	private static string GeneratePDFFromWorkbook(WorkbookWrapper workbook, WorksheetsWrapper worksheets) {

        var PDFSheetNames = new string[] { "MDF Cover Sheet", "MDF Packing List", "MDF Invoice" };
		worksheets[PDFSheetNames].Select();
    
        var tmpFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";

		using var activeSheet = workbook.ActiveSheet;
		activeSheet.ExportAsFixedFormat2(XlFixedFormatType.xlTypePDF, tmpFileName, openAfterPublish: false);

        return tmpFileName;

	}

}
