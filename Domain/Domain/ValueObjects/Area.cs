namespace Domain.ValueObjects;

public readonly struct Area {

    private readonly double _sqr_mm;

    private Area(double sqr_mm) {
        if (sqr_mm < 0) {
            throw new ArgumentOutOfRangeException(nameof(sqr_mm), "Dimension cannot be negative");
        }
        _sqr_mm = sqr_mm;
    }

    public double AsSquareInches() => _sqr_mm / (25.4*25.4);

    public double AsSquareMillimeters() => _sqr_mm;

    public static Area FromSquareInches(double sqr_in) => new(sqr_in * (25.4*25.4));

    public static Area FromSquareMillimeters(double sqr_mm) => new(sqr_mm);

    public static Dimension Sqrt(Area area) => Dimension.FromMillimeters(Math.Sqrt(area.AsSquareMillimeters()));

    public static Area operator +(Area area1, Area area2) => FromSquareMillimeters(area1.AsSquareMillimeters() + area2.AsSquareMillimeters());

}