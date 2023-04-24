using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.Shared.Services;
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

    private string? _generatedFile = null;
    public string? GeneratedFile {
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
    }

    public async Task GeneratePDF() {

        IsGeneratingPDF = true;

        await _cncReleaseDecorator.LoadDataFromFile(Model.ReportFilePath, Model.OrderDate, Model.CustomerName, Model.VendorName);

        var doc = Document.Create(_cncReleaseDecorator.Decorate);

        var outputFilePath = _fileReader.GetAvailableFileName(Model.OutputDirectory, Model.FileName, "pdf"); 
        doc.GeneratePdf(outputFilePath);
        GeneratedFile = outputFilePath;

        IsGeneratingPDF = false;

    }

}
