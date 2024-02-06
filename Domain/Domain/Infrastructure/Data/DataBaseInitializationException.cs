namespace Domain.Infrastructure.Data;

public class DataBaseInitializationException : Exception {

    public DataBaseInitializationException(Exception innerException)
        : base($"Could not initialize database - {innerException.Message}", innerException) {
    }


}