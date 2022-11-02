namespace ApplicationCore.Features.Orders.Domain;

internal interface IPriceAdjustment {

    decimal ApplyPriceAdjustment(decimal initial);

}
