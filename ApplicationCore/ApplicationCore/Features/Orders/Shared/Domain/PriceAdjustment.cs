namespace ApplicationCore.Features.Orders.Shared.Domain;

internal interface IPriceAdjustment {

    decimal ApplyPriceAdjustment(decimal initial);

}
