using System.Xml.Linq;

namespace OrderExporting.CNC.Programs.WSXML.Report;

public record Part(string Id, string LabelId, string Name, string Description, double Width, double Length, IEnumerable<string> PatternScheduleIds, Dictionary<string, string> Variables) {

    public static Part FromXElement(XElement element) {

        Dictionary<string, string> variables = new();

        foreach (var v in element.Elements("Var")) {
            AddVariable(v, variables);
        }

        return new(element.AttributeValue("ID"),
                   element.AttributeValue("LabelID"),
                   element.ElementValue("Name"),
                   element.ElementValue("Description"),
                   element.ElementDouble("FinishedWidth"),
                   element.ElementDouble("FinishedLength"),
                   element.AttributeValue("PatSchID").Split(' '),
                   variables);

    }

    private static void AddVariable(XElement element, IDictionary<string, string> variables) {

        string name = "";
        foreach (var subelement in element.Descendants()) {

            switch (subelement.Name.ToString()) {

                case "Name":
                    name = subelement.Value;
                    break;

                case "Value":
                    variables.TryAdd(name, subelement.Value);
                    return;

            }

        }

    }

}
