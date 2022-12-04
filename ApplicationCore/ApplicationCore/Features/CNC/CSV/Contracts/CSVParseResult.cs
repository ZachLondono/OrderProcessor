namespace ApplicationCore.Features.CNC.CSV.Contracts;

public class CSVReadResult {

    public IEnumerable<CSVToken> Tokens { get; set; } = Enumerable.Empty<CSVToken>();

}