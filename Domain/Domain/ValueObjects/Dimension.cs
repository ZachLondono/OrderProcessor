namespace Domain.ValueObjects;

public readonly struct Dimension : IComparable {

    private readonly double _mm;

    private Dimension(double mm) {
        if (mm < 0) {
            throw new ArgumentOutOfRangeException(nameof(mm), "Dimension cannot be negative");
        }
        _mm = mm;
    }

    public double AsInches() => _mm / 25.4;

    public double AsMillimeters() => _mm;

    public override string ToString() {
        return $"{{{AsInches()}\",  {AsMillimeters()}mm}}";
    }

    public Dimension RoundToInchMultiple(double factor) {

        double roundedValue = Math.Round(AsInches() / (double)factor, MidpointRounding.AwayFromZero) * (double)factor;

        return FromInches(roundedValue);

    }

    public Fraction AsInchFraction(double accuracy = 0.000001) {

        // https://stackoverflow.com/questions/5124743/algorithm-for-simplifying-decimal-to-fractions/42085412#42085412

        if (accuracy <= 0.0 || accuracy >= 1.0) {
            throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be > 0 and < 1.");
        }

        double value = AsInches();

        int sign = Math.Sign(value);

        if (sign == -1) {
            value = Math.Abs(value);
        }

        // Accuracy is the maximum relative error; convert to absolute maxError
        double maxError = sign == 0 ? accuracy : value * accuracy;

        int n = (int)Math.Floor(value);
        value -= n;

        if (value < maxError) {
            return new Fraction(sign * n, 1);
        }

        if (1 - maxError < value) {
            return new Fraction(sign * (n + 1), 1);
        }

        double z = value;
        int previousDenominator = 0;
        int denominator = 1;
        int numerator;

        do {
            z = 1.0 / (z - (int)z);
            int temp = denominator;
            denominator = denominator * (int)z + previousDenominator;
            previousDenominator = temp;
            numerator = Convert.ToInt32(value * denominator);
        } while (Math.Abs(value - (double)numerator / denominator) > maxError && z != (int)z);

        return new Fraction((n * denominator + numerator) * sign, denominator);
    }

    public int CompareTo(object? obj) {

        if (obj == null || obj is not Dimension dim) {
            return 1;
        }

        return AsMillimeters().CompareTo(dim.AsMillimeters());

    }

    public static Dimension FromInches(double inches) => new(inches * 25.4);

    public static Dimension FromMillimeters(double mm) => new(mm);

    public static Dimension Zero => new(0);

    public static bool operator ==(Dimension dim1, Dimension dim2) => dim1.AsMillimeters() == dim2.AsMillimeters();

    public static bool operator !=(Dimension dim1, Dimension dim2) => dim1.AsMillimeters() != dim2.AsMillimeters();

    public static Dimension operator *(Dimension dim1, Dimension dim2) => FromMillimeters(dim1.AsMillimeters() * dim2.AsMillimeters());

    public static Dimension operator *(Dimension dim1, int mult) => FromMillimeters(dim1.AsMillimeters() * mult);

    public static Dimension operator *(int mult, Dimension dim1) => FromMillimeters(mult * dim1.AsMillimeters());

    public static Dimension operator *(Dimension dim1, double mult) => FromMillimeters(dim1.AsMillimeters() * mult);

    public static Dimension operator *(double mult, Dimension dim1) => FromMillimeters(mult * dim1.AsMillimeters());

    public static Dimension operator /(Dimension dim1, Dimension dim2) => FromMillimeters(dim1.AsMillimeters() / dim2.AsMillimeters());

    public static Dimension operator /(Dimension dim1, int divosor) => FromMillimeters(dim1.AsMillimeters() / divosor);

    public static Dimension operator /(Dimension dim1, double divosor) => FromMillimeters(dim1.AsMillimeters() / divosor);

    public static Dimension operator +(Dimension dim1, Dimension dim2) => FromMillimeters(dim1.AsMillimeters() + dim2.AsMillimeters());

    public static Dimension operator -(Dimension dim1, Dimension dim2) => FromMillimeters(dim1.AsMillimeters() - dim2.AsMillimeters());

    public static bool operator >(Dimension dim1, Dimension dim2) => dim1.AsMillimeters() > dim2.AsMillimeters();

    public static bool operator <(Dimension dim1, Dimension dim2) => dim1.AsMillimeters() < dim2.AsMillimeters();

    public static Dimension Sqrt(Dimension dim) => FromMillimeters(Math.Sqrt(dim.AsMillimeters()));

    public static Dimension Pow(Dimension dim1, Dimension dim2) => FromMillimeters(Math.Pow(dim1.AsMillimeters(), dim2.AsMillimeters()));

    public static Dimension CeilingMM(Dimension val) => FromMillimeters(Math.Ceiling(val.AsMillimeters()));

    public static Dimension CeilingIN(Dimension val) => FromInches(Math.Ceiling(val.AsInches()));

    public override bool Equals(object? obj) {

        if (obj is not Dimension dim) {
            return false;
        }

        if (dim == this) {
            return true;
        }

        return false;

    }

    public override int GetHashCode() {
        return _mm.GetHashCode();
    }

    public static bool operator <=(Dimension left, Dimension right) => left.AsMillimeters() <= right.AsMillimeters();

    public static bool operator >=(Dimension left, Dimension right) => left.AsMillimeters() >= right.AsMillimeters();

}