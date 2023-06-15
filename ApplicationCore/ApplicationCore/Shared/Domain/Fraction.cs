namespace ApplicationCore.Shared.Domain;

public readonly struct Fraction {

    public int N { get; }
    public int D { get; }
    public Fraction(int n, int d) {
        N = n;
        D = d;
    }

    public override string ToString() {
        if (N == 0) return "0";
        int whole = N / D;
        int n = N - whole * D;
        if (whole == 0) return $"{n}/{D}";
        else if (n != 0) return $"{whole} {n}/{D}";
        else return whole.ToString();
    }

}