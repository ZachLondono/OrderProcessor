using System.Xml.Linq;

namespace ApplicationCore.Shared.CNC.WSXML;

public static class Extensions {

    public static string AttributeValue(this XElement element, string name) => element.Attribute(name)?.Value ?? "";

    public static string ElementValue(this XElement element, string name) => element.Element(name)?.Value ?? "";

    public static double AttributeDouble(this XElement element, string name) => double.Parse(element.AttributeValue(name));

    public static double ElementDouble(this XElement element, string name) => double.Parse(element.ElementValue(name));

}
