using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Domain;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Services;

public class ExtWriter : IExtWriter {

    private static readonly string _validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_, ";
    private readonly List<Dictionary<string, string>> _records = new();

    public void AddRecord(JobDescriptor job) => _records.Add(GetRecord(job));
    public void AddRecord(LevelVariableOverride variables) => _records.Add(GetRecord(variables));
    public void AddRecord(LevelDescriptor level) => _records.Add(GetRecord(level));
    public void AddRecord(ProductRecord product) => _records.Add(GetRecord(product));

    public void Clear() {
        _records.Clear();
    }

    public string WriteFile(string outputDirectory, string jobName) {

        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture) {
            ShouldQuote = args => false
        };

        string filePath = Path.Combine(outputDirectory, $"{RemoveIllegalJobNameCharacters(TruncateString(jobName, 30))}.ext");

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, configuration);

        foreach (var record in _records) {
            foreach (var field in record) csv.WriteField($"{field.Key.Trim()}={field.Value.Trim()}");
            csv.NextRecord();
        }

        return filePath;

    }

    public static Dictionary<string, string> GetRecord(JobDescriptor job) {

        var jobName = RemoveIllegalJobNameCharacters(TruncateString(job.Job, 30));

        var fields = new Dictionary<string, string>() {
            { "KEY", "XD" },
            { "LEVELID", job.LevelId.ToString() },
            { "JOB", jobName },
            { "DATE", job.Date.ToShortDateString() },
            { "INFO1", job.Info1 }
        };

        if (!string.IsNullOrEmpty(job.Catalog)) fields.Add("PCAT", job.Catalog);
        if (!string.IsNullOrEmpty(job.Materials)) fields.Add("MCAT", job.Materials);
        if (!string.IsNullOrEmpty(job.Fronts)) fields.Add("SCAT", job.Fronts);
        if (!string.IsNullOrEmpty(job.Hardware)) fields.Add("HCAT", job.Hardware);

        return fields;
    }

    public static Dictionary<string, string> GetRecord(LevelVariableOverride variables) {
        var values = new Dictionary<string, string> {
            { "KEY", "LV" },
            { "LEVELID", variables.LevelId.ToString() },
            { "UNT", ((int)variables.Units).ToString() }
        };

        foreach (var parameter in variables.Parameters) {
            values.TryAdd(parameter.Key, parameter.Value);
        }

        return values;
    }

    public static Dictionary<string, string> GetRecord(LevelDescriptor level) {

        var levelName = RemoveIllegalJobNameCharacters(TruncateString(level.Name, 60));

        var fields = new Dictionary<string, string>() {
            { "KEY", "LD" },
            { "LEVELID", level.LevelId.ToString() },
            { "PARENTID", level.ParentId.ToString() },
            { "LEVELNAME", levelName },
        };

        if (!string.IsNullOrEmpty(level.Catalog)) fields.Add("PCAT", level.Catalog);
        if (!string.IsNullOrEmpty(level.Materials)) fields.Add("MCAT", level.Materials);
        if (!string.IsNullOrEmpty(level.Fronts)) fields.Add("SCAT", level.Fronts);
        if (!string.IsNullOrEmpty(level.Hardware)) fields.Add("HCAT", level.Hardware);

        return fields;
    }

    public static Dictionary<string, string> GetRecord(ProductRecord product) {
        var values = new Dictionary<string, string> {
            { "KEY", "PR" },
            { "PARENTID", product.ParentId.ToString() },
            { "NAME", product.Name },
            { "UNT", ((int)product.Units).ToString() },
            { "WIDTH", "0" },
            { "HEIGHT", "0" },
            { "DEPTH", "0" },
            { "QTY", product.Qty.ToString() },
            { "POS", product.Pos.ToString() },
            { "CABCOM", SanitizeAndTruncateComment(product.Comment) },
            { "CABCOM2", "" },
            { "SEQTEXT", TruncateString(product.SeqText, 20) }
        };

        if (product.CustomSpec) values.Add("CUSTSPEC", "1");

        foreach (var parameter in product.Parameters) {
            values.TryAdd(parameter.Key, parameter.Value);
        }

        return values;
    }

    private static string SanitizeAndTruncateComment(string comment) {

        string specialChars = """`~!@#$%^&*()_+-=[]{}\|:'".></?""";

        // Allow , and ;
        string sanitized = new string(comment.Where(c => !specialChars.Contains(c)).ToArray()).Replace(",", ";");

        return TruncateString(sanitized, 60);

    }

    private static string TruncateString(string str, int maxLength) => str.Length > maxLength ? str[..maxLength] : str;

    private static string RemoveIllegalJobNameCharacters(string jobName) => new(jobName.Where(_validCharacters.Contains).ToArray());

}
