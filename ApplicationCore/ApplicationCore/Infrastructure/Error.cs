namespace ApplicationCore.Infrastructure;

public class Error {

    public string Message { get; set; } = string.Empty;

    public Error() { }

    public Error(string message) {
        Message = message;
    }

}
