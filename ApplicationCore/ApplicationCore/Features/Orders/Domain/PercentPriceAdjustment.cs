namespace ApplicationCore.Features.Orders.Domain;

public class PercentPriceAdjustment : IPriceAdjustment {

    public decimal Percent { get; }

    public PercentPriceAdjustment(decimal percent) {
        Percent = percent;
    }

    public decimal ApplyPriceAdjustment(decimal initial) => initial * (1+Percent);

}
