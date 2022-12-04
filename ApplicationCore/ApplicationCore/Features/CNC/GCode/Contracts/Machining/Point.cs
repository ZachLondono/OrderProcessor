using System;

namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public record Point()
{

    public double X { get; init; }
    public double Y { get; init; }

    public Point(double x, double y) : this()
    {
        X = x;
        Y = y;
    }

}
