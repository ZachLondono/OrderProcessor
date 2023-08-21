using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Shared.Services;
using QuestPDF.Fluent;

namespace ApplicationCore.Features.CNC.ReleasePDF.Dialog;

internal class ReleasePDFDialogViewModel {

    public Action? OnPropertyChanged { get; set; }

    public Model Model { get; set; } = new();

    private string? _error = null;
    public string? Error {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isGeneratingPDF = false;
    public bool IsGeneratingPDF {
        get => _isGeneratingPDF;
        set {
            _isGeneratingPDF = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private List<string> _generatedFile = new();
    public List<string> GeneratedFiles {
        get => _generatedFile;
        set {
            _generatedFile = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private readonly ICNCReleaseDecorator _cncReleaseDecorator;
    private readonly IFileReader _fileReader;

    public ReleasePDFDialogViewModel(ICNCReleaseDecorator cncReleaseDecorator, IFileReader fileReader) {
        _cncReleaseDecorator = cncReleaseDecorator;
        _fileReader = fileReader;
        Model.OutputDirectory = @"R:\Door Orders\Door Programs";
    }

    public async Task GeneratePDF() {

        Error = null;

        if (string.IsNullOrEmpty(Model.ReportFilePath)) {
            Error = "WSXML report is required";
            return;
        }

        if (Path.GetExtension(Model.ReportFilePath) != ".xml") {
            Error = "Invalid WSXML file";
            return;
        }

        if (!File.Exists(Model.ReportFilePath)) {
            Error = "Report file not found";
            return;
        }

        string[] outputDirectories = Model.OutputDirectory.Split(';');

        foreach (var directory in outputDirectories) {
            if (!Directory.Exists(directory)) {
                Error = "Output directory does not exist";
                return;
            }
        }

        IsGeneratingPDF = true;

        try {

            await _cncReleaseDecorator.LoadDataFromFile(Model.ReportFilePath, Model.OrderDate, Model.CustomerName, Model.VendorName);

            var doc = Document.Create(_cncReleaseDecorator.Decorate);

            foreach (var directory in outputDirectories) {
                var outputFilePath = _fileReader.GetAvailableFileName(directory, Model.FileName, "pdf");
                doc.GeneratePdf(outputFilePath);
                GeneratedFiles.Add(outputFilePath);
            }

        } catch {
            Error = "Failed to generate pdf";
            GeneratedFiles = new();
        }

        IsGeneratingPDF = false;

    }

}
