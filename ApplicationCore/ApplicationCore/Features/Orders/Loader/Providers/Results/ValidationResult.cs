namespace ApplicationCore.Features.Orders.Loader.Providers.Results;

public record ValidationResult {

    public bool IsValid { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

}
