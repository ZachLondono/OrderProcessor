using Domain.ValueObjects;
namespace ApplicationCore.Features.Orders.Shared.Domain;

public static class Extensions {

    public static string GetFormatedFraction(this Dimension dimension, double roundingFactor = 0) {

        if (roundingFactor != 0) {
            dimension = dimension.RoundToInchMultiple(roundingFactor);
        }

        var fraction = dimension.AsInchFraction();

        if (fraction.N == 0) {
            return "0";
        }

        int whole = fraction.N / fraction.D;
        int n = fraction.N - (whole * fraction.D);

        if (whole == 0) {
            return $"<span class=\"diagonal-fractions\">{n}/{fraction.D}</span>";
        } else if (n != 0) {
            return $"{whole} <span class=\"diagonal-fractions\">{n}/{fraction.D}</span>";
        } else {
            return whole.ToString();
        }

    }

}