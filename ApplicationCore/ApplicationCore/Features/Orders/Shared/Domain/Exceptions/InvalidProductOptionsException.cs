namespace ApplicationCore.Features.Orders.Shared.Domain.Exceptions;

public class InvalidProductOptionsException : Exception {

    public string Reason { get; init; }

    public InvalidProductOptionsException(string reason) : base(reason) {
        Reason = reason;
    }

}
