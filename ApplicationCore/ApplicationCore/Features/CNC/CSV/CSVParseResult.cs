namespace ApplicationCore.Features.CNC.CSV;

internal class CSVParseResult
{

    public IEnumerable<CSVPart> Parts { get; set; } = Enumerable.Empty<CSVPart>();

    public IEnumerable<ParseMessage> Messages { get; set; } = Enumerable.Empty<ParseMessage>();

    public record ParseMessage(MessageSeverity Severity, string Message);

    public enum MessageSeverity
    {
        Warning,
        Error
    }

}