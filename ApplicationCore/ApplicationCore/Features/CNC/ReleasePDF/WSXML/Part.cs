using System.Xml.Linq;

namespace ApplicationCore.Features.CNC.ReleasePDF.WSXML;

internal record Part(string Id, string LabelId, string Name, string Description, double Width, double Length, IEnumerable<string> PatternScheduleIds, Dictionary<string, string> Variables) {
    public static Part FromXElemnt(XElement element) {

        Dictionary<string, string> variables = new();

        foreach (var v in element.Elements("Var")) {
            AddVariable(v, variables);
        }

        return new(element.AttributeValue("ID"), element.AttributeValue("LabelID"), element.ElementValue("Name"), element.ElementValue("Description"), element.ElementDouble("FinishedLength"), element.ElementDouble("FinishedWidth"), element.AttributeValue("PatSchID").Split(' '), variables);

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
