namespace Domain.ValueObjects;

public readonly struct Fraction(int n, int d) {

    public int N { get; } = n;
    public int D { get; } = d;

    public override string ToString() {
        if (N == 0) return "0";
        int whole = N / D;
        int n = N - whole * D;
        if (whole == 0) return $"{n}/{D}";
        else if (n != 0) return $"{whole} {n}/{D}";
        else return whole.ToString();
    }

}