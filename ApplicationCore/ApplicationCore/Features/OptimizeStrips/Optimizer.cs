using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.OptimizeStrips;

public class Optimizer {

    public static OptimizationResult Optimize(Dimension materialLength, Dimension[] partLengths) {

        List<Dimension[]> parts = [];

        var lengths = partLengths.Where(l => l <= materialLength).ToList();
        var unPlacedLengths = partLengths.Where(l => l > materialLength).ToList();
        
        while (lengths.Count > 0) {
        
            var used = KnapSack(materialLength, lengths.ToArray(), lengths.Count);
            
            foreach (var item in used) {
                lengths.Remove(item);
            }

            parts.Add(used.ToArray());
        
        }

        return new() {
            PartsPerMaterial = parts,
            UnplacedParts = unPlacedLengths
        };

    }

    public static List<Dimension> KnapSack(Dimension capacity, Dimension[] lengths, int itemCount) {

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

    }

}