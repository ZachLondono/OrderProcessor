using System.Xml.Linq;

namespace OrderExporting.CNC.Programs.WSXML.Report;

public record PartLabels(string Id, Dictionary<string, string> Fields) {

    public static PartLabels FromXElement(XElement element) {

        Dictionary<string, string> fields = new();

        string name = "";
        foreach (var subelement in element.Descendants()) {

            switch (subelement.Name.ToString()) {

                case "Name":
                    name = subelement.Value;
                    break;

                case "Value":
                    fields.TryAdd(name, subelement.Value);
                    name = "";
                    break;

            }

        }

        return new(element.AttributeValue("ID"), fields);

    }

}
