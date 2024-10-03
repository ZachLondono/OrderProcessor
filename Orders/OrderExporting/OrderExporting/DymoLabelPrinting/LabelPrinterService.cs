using DymoSDK.Implementations;
using DymoSDK.Interfaces;

namespace OrderExporting.DymoLabelPrinting;

public class LabelPrinterService {

    public static void Initialize() {

        DymoSDK.App.Init();

    }

    public async Task<LabelPrinter?> GetPrinter() {

        try {

            var printers = await DymoPrinter.Instance.GetPrinters();

            if (!printers.Any()) {
                return null;
            }

            var printer = printers.First();

            return new(printer.Name, printer.IsConnected);

        } catch {

            return null;

        }

    }

    public async Task PrintLabels(string printerName, Label[] labels) {

        var labelGroups = labels.GroupBy(l => l.TemplateFile);

        var dymoLabel = DymoLabel.Instance;

        foreach (var labelGroup in labelGroups) {

            var xml = await File.ReadAllTextAsync(labelGroup.Key);

            foreach (var label in labelGroup) {

                dymoLabel.LoadLabelFromXML(xml);

                FillLabelObjects(dymoLabel, label.Fields);

                await DymoPrinter.Instance.PrintLabel(dymoLabel, printerName, label.Quantity);
                
            }

        }

    }

    public byte[] PreviewLabel(Label label) {

        var dymoLabel = DymoLabel.Instance;

        dymoLabel.LoadLabelFromFilePath(label.TemplateFile);

        FillLabelObjects(dymoLabel, label.Fields);

        return dymoLabel.GetPreviewLabel();

    }

    private static void FillLabelObjects(IDymoLabel dymoLabel, IReadOnlyDictionary<string, string> fields) {

        foreach (var obj in dymoLabel.GetLabelObjects()) {

            if (fields.TryGetValue(obj.Name, out string? value)) {

                dymoLabel.UpdateLabelObject(obj, value);

            }

        }

    }

}
