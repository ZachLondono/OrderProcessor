using ApplicationCore.Features.Orders.Domain.ValueObjects;
using static ApplicationCore.Features.Orders.Domain.ValueObjects.Dimension;

namespace ApplicationCore.Features.Orders.Domain;

public static class Extensions {

    public static string GetColor(this Status status) => status switch {
        Status.Pending => "bg-amber-500",
        Status.Released => "bg-blue-500",
        Status.Completed => "bg-green-500",
        _ => "bg-gray-500"
    };

    public static string GetFormatedFraction(this Dimension dimension) {

        var fraction = dimension.AsInchFraction();

        if (fraction.N == 0) return "0";
        int whole = fraction.N / fraction.D;
        int n = fraction.N - (whole * fraction.D);
        if (whole == 0) return $"<span class=\"diagonal-fractions\">{n}/{fraction.D}</span>";
        else if (n != 0) return $"{whole} <span class=\"diagonal-fractions\">{n}/{fraction.D}</span>";
        else return whole.ToString();

    }

}