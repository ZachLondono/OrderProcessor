using System.Xml.Linq;

namespace ApplicationCore.Features.CNC.ReleasePDF.WSXML;

internal record Item(string Id, string Name, string Note, IEnumerable<string> PartIds) {
    public static Item FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.ElementValue("Note"), element.Elements("Part").Select(e => e.AttributeValue("ID")));
}
