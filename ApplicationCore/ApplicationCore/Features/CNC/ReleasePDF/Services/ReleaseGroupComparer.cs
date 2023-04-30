using ApplicationCore.Features.CNC.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.CNC.ReleasePDF.Services;

public class ReleaseGroupComparer : IEqualityComparer<MachineRelease> {

    public bool Equals(MachineRelease? x, MachineRelease? y) {

        if (x is null && y is null) return true;
        if (x is null || y is null) return false;

        var xEnum = x.Programs.GetEnumerator();
        var yEnum = y.Programs.GetEnumerator();

        /**
            Two sets of programs are equal if the following is true
            1) Both sets have the same number of programs
            2) The Nth program in set X has the same material (name, size, thickness, graining) as the Nth program in set Y
            3) The Nth program in set X has the same parts (name and qty) as the Nth program in set Y
            4) If the Nth program in set X has has a face 6 the Nth program in set Y must also
        **/
        while (true) {

            bool xResult = xEnum.MoveNext();
            bool yResult = yEnum.MoveNext();

            if (xResult != yResult) return false;
            if (!xResult) break;

            var xProg = xEnum.Current;
            var yProg = yEnum.Current;

            if (xProg.Material.Name != yProg.Material.Name) return false;
            if (xProg.Material.Width != yProg.Material.Width) return false;
            if (xProg.Material.Length != yProg.Material.Length) return false;
            if (xProg.Material.Thickness != yProg.Material.Thickness) return false;
            if (xProg.Material.IsGrained != yProg.Material.IsGrained) return false;

            if (xProg.HasFace6 != yProg.HasFace6) return false;

            var xParts = xProg.Parts.GroupBy(p => p.Name).Select(g => new PartGroup(g.Key, g.Count()));
            var yParts = yProg.Parts.GroupBy(p => p.Name).Select(g => new PartGroup(g.Key, g.Count()));

            if (!xParts.All(yParts.Contains)) return false;

        }


        // Both tool tables must _similar_ but do not need to be _exactly_ the same.
        // Both tool tables must have the same tools in the same positions, but if one has extra empty tool positions that are not present on the other table, that is fine
        foreach (var position in x.ToolTable.Keys) {

            var tool = x.ToolTable[position];

            if (tool == string.Empty) {

                if (y.ToolTable.TryGetValue(position, out string? yTool)) {
                    if (yTool != string.Empty) {
                        return false;
                    }
                }

            } else if (y.ToolTable.TryGetValue(position, out string? yTool)) {

                if (tool != yTool) return false;

            } else return false;

        }


        return true;

    }

    public int GetHashCode([DisallowNull] MachineRelease obj) => 0;

    public record PartGroup(string Name, int Count);

}


