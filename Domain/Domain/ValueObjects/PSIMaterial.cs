using System.Text.RegularExpressions;

namespace Domain.ValueObjects;

public partial record PSIMaterial(string Side1Color, string Side1FinishType, string Side2Color, string Side2FinishType, string CoreType, double Thickness) {

    public static bool TryParse(string materialName, out PSIMaterial material) {

        /*
         *  PSI Material Format
         *  Grained[2 Spaces][Side 1 Color][1 Space][Mela / Veneer][5 Spaces][Thickness w/ 2 decimal places][1 Space][PB / Ply / Flake / MDF Dr Core][2 Spaces][Side 2 Color][1 Space][Mela / Veneer]
         */

        var regex = PSIMaterialNamePatternRegex();

        material = new("", "", "", "", "", 0);

        materialName = CleanMaterialName(materialName);

        if (!regex.IsMatch(materialName)) return false;

        var split1 = materialName.Split("     ");

        if (split1.Length != 2) return false;

        var leftSplit = split1[0].Split("  ");
        if (leftSplit.Length != 2) return false;

        if (leftSplit[0] != "Grained") return false;

        (string, string) splitColorAndMaterial(string substring) {
            var split = substring.Split(" ");
            if (split.Length < 2) return ("", "");
            var color = string.Join(' ', split[..^1]);
            var finish = split[^1];
            return (color, finish);
        };

        (string side1Color, string side1Finish) = splitColorAndMaterial(leftSplit[1]);

        var rightSplit = split1[1].Split("  ");

        (string side2Color, string side2Finish) = splitColorAndMaterial(rightSplit[1]);

        if (side1Color == string.Empty || side1Finish == string.Empty ||
            side2Color == string.Empty || side2Finish == string.Empty) return false;

        var rightSplit2 = rightSplit[0].Split(' ');
        if (rightSplit2.Length != 2) return false;
        string thicknessStr = rightSplit2[0];
        string coreType = rightSplit2[1];

        if (!double.TryParse(thicknessStr, out double thickness)) {
            thickness = 0;
        }

        material = new(side1Color, side1Finish, side2Color, side2Finish, coreType, thickness);

        return true;

    }

    public static string CleanMaterialName(string materialName) {

        if (!materialName.Contains("(tops)")) {
            return materialName;
        }

        return materialName.Replace(" (tops) ", " ")
                           .Replace(" (tops)", " ")
                           .Replace("(tops) ", " ")
                           .Replace("(tops)", "")
                           .Trim();

    }

    public string GetSimpleName() {

        if (Side1Color == Side2Color && Side1FinishType == Side2FinishType) {
            return $"{Side1Color} {Side1FinishType}";
        } else {
            return $"{Side1Color} {Side1FinishType} / {Side2Color} {Side2FinishType}";
        }

    }

    public string GetLongName() {

        return $"Grained  {Side1Color.ToUpperInvariant()} {Side1FinishType.ToUpperInvariant()}     {Thickness:0.00} {CoreType.ToUpperInvariant()}  {Side2Color.ToUpperInvariant()} {Side2FinishType.ToUpperInvariant()}";

    }

    [GeneratedRegex("Grained+\\s{2}(.+)\\s{1}[A-Za-z]+\\s{5}[0-9]*\\.[0-9]+\\s{1}[A-Za-z]+\\s{2}(.+)\\s{1}[A-Za-z]+", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex PSIMaterialNamePatternRegex();

}