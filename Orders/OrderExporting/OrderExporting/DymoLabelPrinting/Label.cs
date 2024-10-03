namespace OrderExporting.DymoLabelPrinting;

public record Label(string TemplateFile, int Quantity, IReadOnlyDictionary<string, string> Fields);
