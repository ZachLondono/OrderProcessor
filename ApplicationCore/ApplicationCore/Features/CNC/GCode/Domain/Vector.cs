namespace ApplicationCore.Features.CNC.GCode.Domain;

struct Vector {

    public double X { get; set; }
    public double Y { get; set; }

    public double Magnitude => Math.Sqrt(X * X + Y * Y);

    public Vector GetNormal() {
        var magnitude = Magnitude;
        return new(X / magnitude, Y / magnitude);
    }

    public Vector(double x, double y) {
        X = x;
        Y = y;
    }

}