namespace ApplicationCore.Infrastructure;

public class Error {

    public required string Title { get; set; } = string.Empty;

    public required string Details { get; set; } = string.Empty;

    public Error() { }

}
