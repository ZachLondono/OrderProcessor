using System.Xml.Linq;
using ApplicationCore.Shared.CNC.WSXML;

namespace ApplicationCore.Shared.CNC.WSXML.Report;

public record MaterialRecord(string Id, string Name, double XDim, double YDim, double ZDim) {
    public static MaterialRecord FromXElement(XElement element) => new(element.AttributeValue("ID"), element.ElementValue("Name"), element.ElementDouble("XDim"), element.ElementDouble("YDim"), element.ElementDouble("ZDim"));
}
