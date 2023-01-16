using ApplicationCore.Features.Orders.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public class AmountPriceAdjustment : IPriceAdjustment {

    public decimal Amount { get; }

    public AmountPriceAdjustment(decimal amount) {
        Amount = amount;
    }

    public decimal ApplyPriceAdjustment(decimal initial) => initial + Amount;

}
