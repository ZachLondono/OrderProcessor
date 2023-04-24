namespace ApplicationCore.Features.CNC.Contracts;

public class PDFGenerationResult {

    public IEnumerable<string> FilePaths { get; set; } = Enumerable.Empty<string>();

}
