using Domain.ValueObjects;
using Google.OrTools.Algorithms;

namespace ApplicationCore.Features.OptimizeStrips;

public class Optimizer {

    public static OptimizationResult Optimize(Dimension materialLength, Dimension[] partLengths) {

        var lengths = partLengths.Where(l => l <= materialLength).ToArray();
        var unPlacedLengths = partLengths.Where(l => l > materialLength).ToList();

        var parts = KnapSack(materialLength, lengths);

        return new() {
            PartsPerMaterial = parts,
            UnplacedParts = unPlacedLengths
        };

    }

    public static List<Dimension[]> KnapSack(Dimension capacity, Dimension[] lengths) {

        KnapsackSolver solver = new KnapsackSolver(
            KnapsackSolver.SolverType.KNAPSACK_MULTIDIMENSION_BRANCH_AND_BOUND_SOLVER, "MaterialKnapsackSolver");

        var weights = new long[1, lengths.Length];
        for (int i = 0; i < lengths.Length; i++) {
            weights[0, i] = (long)(lengths[i].AsMillimeters() * 100); // in 1000s of a millimeter
        }

        long[] capacities = { (long)(capacity.AsMillimeters() * 100) };

        List<Dimension[]> parts = [];

        while (weights.Length > 0) {

            long[] values = new long[weights.Length];
            for (int i = 0; i < weights.Length; i++) {
                values[i] = weights[0, i];
            }

            solver.Init(values, weights, capacities);
            long computedValue = solver.Solve();

            List<long> packedItems = [];
            List<long> notPacked = [];
            for (int i = 0; i < weights.Length; i++) {
                if (solver.BestSolutionContains(i)) {
                    packedItems.Add(weights[0, i]);
                } else {
                    notPacked.Add(weights[0, i]);
                }
            }

            weights = new long[1, notPacked.Count];
            for (int i = 0; i < weights.Length; i++) {
                weights[0, i] = notPacked[i];
            }

            parts.Add(packedItems.Select(v => Dimension.FromMillimeters(((double)v) / (double)100)).ToArray());

        }

        return parts;

        /*
        if (itemCount == 0 || capacity == Dimension.Zero) {
            return [];
        }

        // If weight of the last item is 
        // more than Knapsack capacity, 
        // then this item cannot be 
        // included in the optimal solution 
        if (lengths[itemCount - 1] > capacity) {
            return KnapSack(capacity, lengths, itemCount - 1);
        }

        // Return the maximum of two cases: 
        // (1) last item included 
        // (2) not included
        var includeItem = KnapSack(capacity - lengths[itemCount - 1], lengths, itemCount - 1);
        includeItem.Add(lengths[itemCount - 1]);

        var notIncluded = KnapSack(capacity, lengths, itemCount - 1);

        if (includeItem.Sum(d => d.AsMillimeters()) > notIncluded.Sum(d => d.AsMillimeters())) {
            return includeItem;
        } else {
            return notIncluded;
        }
        */

    }

}