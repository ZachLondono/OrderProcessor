using System.Xml.Linq;
using ApplicationCore.Shared.CNC.WSXML;

namespace ApplicationCore.Shared.CNC.WSXML.Report;

public record Item(string Id, string Name, string Note, IEnumerable<string> PartIds) {
    public static Item FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.ElementValue("Note"), element.Elements("Part").Select(e => e.AttributeValue("ID")));
}
