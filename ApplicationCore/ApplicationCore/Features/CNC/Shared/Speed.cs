namespace ApplicationCore.Features.CNC.Shared;

public record Speed {

    private readonly double _mmPerSecond = 0;

    private Speed(double mmPerSecond) {
        _mmPerSecond = mmPerSecond;
    }

    public static Speed FromMillimetersPerSecond(double mmPerSecond) => new(mmPerSecond);

    public static Speed FromInchesPerSecond(double inPerSecond) => new(inPerSecond * 25.4);

    public double AsMillimetersPerSecond() => _mmPerSecond;

    public double AsInchesPerSecond() => _mmPerSecond / 25.4;

    public static Speed operator *(Speed dim1, int mult) => FromMillimetersPerSecond(dim1.AsMillimetersPerSecond() * mult);

    public static Speed operator *(int mult, Speed dim1) => FromMillimetersPerSecond(mult * dim1.AsMillimetersPerSecond());

    public static Speed operator *(Speed dim1, double mult) => FromMillimetersPerSecond(dim1.AsMillimetersPerSecond() * mult);

    public static Speed operator *(double mult, Speed dim1) => FromMillimetersPerSecond(mult * dim1.AsMillimetersPerSecond());

    public static Speed operator /(Speed dim1, int divosor) => FromMillimetersPerSecond(dim1.AsMillimetersPerSecond() / divosor);

    public static Speed operator /(Speed dim1, double divosor) => FromMillimetersPerSecond(dim1.AsMillimetersPerSecond() / divosor);

    public static Speed operator +(Speed dim1, Speed dim2) => FromMillimetersPerSecond(dim1.AsMillimetersPerSecond() + dim2.AsMillimetersPerSecond());

    public static Speed operator -(Speed dim1, Speed dim2) => FromMillimetersPerSecond(dim1.AsMillimetersPerSecond() - dim2.AsMillimetersPerSecond());

    public override string ToString() => $"{{{AsMillimetersPerSecond()} mm/s, {AsInchesPerSecond()} in/s}}";

}
