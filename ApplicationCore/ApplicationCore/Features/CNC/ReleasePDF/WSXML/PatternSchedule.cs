using System.Xml.Linq;

namespace ApplicationCore.Features.CNC.ReleasePDF.WSXML;

internal record PatternSchedule(string Id, string Name, IEnumerable<Pattern> Patterns) {
    public static PatternSchedule FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.Elements("Pattern").Select(Pattern.FromXElement));
}
