using System.Text.RegularExpressions;

namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;

internal static partial class DateParser {

    [GeneratedRegex(@"(?<month>\d{1,2})\.(?<day>\d{1,2})\.(?<year>\d{2}|\d{4})", RegexOptions.Compiled)]
    private static partial Regex DateRegex();

    public static bool TryParseDate(string input, out DateTime date) {

        var match = DateRegex().Match(input);

        if (match.Success &&
            int.TryParse(match.Groups["day"].Value, out int day) &&
            int.TryParse(match.Groups["month"].Value, out int month) &&
            int.TryParse(match.Groups["year"].Value, out int year)) {

            if (year < 100)
                year += 2000;

            date = new DateTime(year, month, day);

            return true;
        }

        date = DateTime.MinValue;
        return false;

    }
}
