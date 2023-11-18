using ApplicationCore.Features.OpenDoorOrders;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.CNC;
using ApplicationCore.Shared.CNC;
using ApplicationCore.Shared.CNC.Job;
using ApplicationCore.Shared.CNC.ReleasePDF;
using ApplicationCore.Shared.Components.ProgressModal;
using ApplicationCore.Shared.Services;
using CADCodeProxy.CSV;
using Microsoft.Office.Interop.Excel;
using QuestPDF.Fluent;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using UglyToad.PdfPig.Writer;
using Action = System.Action;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;

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

        await GenerateCSVTokens(_generator, DoorOrder);

    }

    public async Task GenerateCSVTokens(CNCPartGCodeGenerator generator, DoorOrder doorOrder) {

        if (!File.Exists(doorOrder.OrderFile) || Path.GetExtension(doorOrder.OrderFile) != ".xlsm") {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Invalid door order file"));
            return;
        }

        ExcelApp? app = null;
        Workbooks? workbooks = null;
        Workbook? workbook = null;
        Sheets? worksheets = null;
        Worksheet? dataSheet = null;
        Worksheet? orderSheet = null;
        string? tmpFileName = null;

        try {

            var batches = await Task.Run(() => {
    
    			app = new ExcelApp() {
    				Visible = false,
    				DisplayAlerts = false
    			};
    
    			workbooks = app.Workbooks;
    			workbook = workbooks.Open(doorOrder.OrderFile, ReadOnly: true);
    			worksheets = workbook.Worksheets;
    
    			dataSheet = worksheets["MDF Door Data"];
    			var exportDirectory = dataSheet.Range["ExportFile"].Value2;
    
    			var tokenFile = Path.Combine(exportDirectory, $"{doorOrder.OrderNumber} - DoorTokens.csv");

                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Generating CSV Token File"));
                var fileName = Path.GetFileName(doorOrder.OrderFile);

                var server = new NamedPipeServer();
                var serverTask = Task.Run(server.Start);

                _ = app.GetType()
                        .InvokeMember("Run", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod, null, app, new object[] { $"'{fileName}'!SilentDoorProcessing" });

                server.Stop();

                tmpFileName = GeneratePDFFromWorkbook(workbook, worksheets);

                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Reading CSV Token File"));
                var batches = new CSVTokenReader().ReadBatchCSV(tokenFile);

                return batches;

            });


            List<ICNCReleaseDecorator> decorators = new();
            foreach (var batch in batches) {
                var job = await generator.GenerateGCode(batch, doorOrder.Customer, doorOrder.Vendor, DateTime.Today, DateTime.Today);
                if (job is not null) {
                    var decorator = _releaseDecoratorFactory.Create(job);
                    decorators.Add(decorator);
                }
            }

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Creating CNC Release Document"));
            var pdfData = await Task.Run(() => {

                var document = Document.Create(doc => {

                    foreach (var decorator in decorators) {
                        decorator.Decorate(doc);
                    }

                });

                return document.GeneratePdf();

            });

            var fileDatas = new List<byte[]>();

            if (tmpFileName is not null) {
                var mdfReleasePagesData = await File.ReadAllBytesAsync(tmpFileName);
                fileDatas.Add(mdfReleasePagesData);
            }

            fileDatas.Add(pdfData);

            var mergedDocument = await Task.Run(() => PdfMerger.Merge(fileDatas));

            var mergedFilePath = _fileReader.GetAvailableFileName(@"C:\Users\Zachary Londono\Desktop\TestOutput", $"{doorOrder.OrderNumber} CUTLIST", ".pdf");
            await File.WriteAllBytesAsync(mergedFilePath, mergedDocument);

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, mergedFilePath));

		} finally {

                
            if (tmpFileName is not null && File.Exists(tmpFileName)) File.Delete(tmpFileName);

            if (dataSheet is not null) _ = Marshal.ReleaseComObject(dataSheet);
            if (orderSheet is not null) _ = Marshal.ReleaseComObject(orderSheet);
            if (worksheets is not null) _ = Marshal.ReleaseComObject(worksheets);

            workbook?.Close(SaveChanges: false);
            workbooks?.Close();
            app?.Quit();

            if (workbook is not null) _ = Marshal.ReleaseComObject(workbook);
            if (workbooks is not null) _ = Marshal.ReleaseComObject(workbooks);
            if (app is not null) _ = Marshal.ReleaseComObject(app);

            workbook = null;
            workbooks = null;
            app = null;

            // Clean up COM objects, calling these twice ensures it is fully cleaned up.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

    }

	private static string GeneratePDFFromWorkbook(Workbook workbook, Sheets worksheets) {

		var PDFSheetNames = new string[] { "MDF Cover Sheet", "MDF Packing List", "MDF Invoice" };
		worksheets[PDFSheetNames].Select();
    
        var tmpFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";
		Worksheet activeSheet = workbook.ActiveSheet;
		activeSheet.ExportAsFixedFormat2(XlFixedFormatType.xlTypePDF, tmpFileName, OpenAfterPublish: false);

        return tmpFileName;

	}
}

public class NamedPipeServer {

    private bool listening = false;
    private ConcurrentQueue<PipeMessage> Messages = new();

    public NamedPipeServer() {
    }

    public void Start() {

        listening = true;

        int i;
        Thread[] servers = new Thread[10];

        for (i = 0; i < 10; i++) {
            servers[i] = new Thread(ServerThread);
            servers[i].Start();
        }

        Thread.Sleep(250);
        while (listening) {

            for (int j = 0; j < 10; j++) {

                if (servers[j] == null) {
                    continue;
                }

                if (!servers[j].Join(250)) {
                    continue;
                    //i--;    // decrement the thread watch count
                }

                if (listening) {
                    servers[j] = null;
                    servers[j] = new Thread(ServerThread);
                    servers[j].Start();
                }
            }
        }

        foreach (Thread t in servers) {
            t.Join();
        }

    }

    public void Stop() => listening = false;

    public void ServerThread(object data) {
        NamedPipeServerStream pipeServer = new NamedPipeServerStream("MDFDoorPipe", PipeDirection.InOut, 10, PipeTransmissionMode.Message);

        int threadId = Thread.CurrentThread.ManagedThreadId;

        bool isConnected = false;
        var request = pipeServer.BeginWaitForConnection((a) => {
            isConnected = true;
        }, null);

        while (listening && !isConnected) { }

        if (!isConnected) {
            pipeServer.Close();
            return;
        }

        try {


            List<byte> intext = new List<byte>();
            do {

                if (!listening) break;

                byte[] x = new byte[1024 * 16];
                int read = 0;
                read = pipeServer.Read(x);
                Array.Resize(ref x, read);
                intext.AddRange(x);

            } while (!pipeServer.IsMessageComplete);

            string receivedText = System.Text.Encoding.UTF8.GetString(intext.ToArray());

            var msgParts = receivedText.Split(';');
            Messages.Enqueue(new(msgParts[0], msgParts[1], msgParts[2]));

            string sentText = "OK";
            pipeServer.Write(System.Text.Encoding.UTF8.GetBytes(sentText));

        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e) {
            Console.WriteLine("ERROR: {0}", e.Message);
        }
        //pipeServer.WaitForPipeDrain();
        pipeServer.Close();
    }

    public record PipeMessage(string Type, string MessageA, string MessageB);

}

