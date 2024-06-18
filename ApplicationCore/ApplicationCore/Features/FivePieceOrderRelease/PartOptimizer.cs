namespace ApplicationCore.Features.FivePieceOrderRelease;

public class PartOptimizer {

    public record OptimizationResult(double[] UnplacedParts, double[][] OptimizedStrips);

    public record OptimizationProgress(int PercentComplete, int PartsLeft);

    public Action<OptimizationProgress>? OnProgress { get; }

    public OptimizationResult OptimizeStrips(double stripLength, double kerf, double[] parts) {

        parts = parts.OrderDescending().ToArray();

        List<List<double>> strips = new();

        int originalPartCount = parts.Length;

        while (true) {

            var (strip, leftOverParts) = CalculateStrip(stripLength, parts, kerf);

            if (strip.Count != 0) {
                strips.Add(strip);
            }

            parts = leftOverParts.ToArray();

            OnProgress?.Invoke(new(1 - (parts.Length / originalPartCount), parts.Length));

            if (strip.Count == 0 || leftOverParts.Count == 0) {
                break;
            }

        }

        double[][] finalStrips = new double[strips.Count][];
        for (int i = 0; i < strips.Count; i++) {
            finalStrips[i] = strips[i].ToArray();
        }

        return new(parts, finalStrips);

    }

    private static (List<double> strip, List<double> leftOverParts) CalculateStrip(double stripLength, double[] parts, double kerf) {

        List<double> strip = [];
        List<double> leftOver = [];

        double currentLength = 0;
        foreach (var part in parts) {

            if (part + currentLength + kerf < stripLength) {

                currentLength += part + kerf;
                strip.Add(part);

            } else {

                leftOver.Add(part);

            }

        }

        return (strip, leftOver);

    }

}