using System.Xml.Linq;
using ApplicationCore.Shared.CNC.WSXML;

namespace ApplicationCore.Shared.CNC.WSXML.Report;

public record PatternSchedule(string Id, string Name, IEnumerable<Pattern> Patterns) {
    public static PatternSchedule FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.Elements("Pattern").Select(Pattern.FromXElement));
}
