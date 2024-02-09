using OrderExporting.CNC.Programs.Domain;
using System.Xml.Linq;

namespace OrderExporting.CNC.Programs.WSXML.Report;

public record PatternPart(string PartId, IEnumerable<PatternPartLocation> Locations) {
    public static IEnumerable<PatternPart> FromXElement(XElement patternElement) {

        return patternElement.Elements("NestPart")
                        .GroupBy(nestPart => nestPart.AttributeValue("PartID"))
                        .Select(group =>
                            new PatternPart(group.Key,
                                            group.Select(nestPart => {
                                                var insert = nestPart.Element("Insert");
                                                bool isRotated = nestPart.ElementValue("Rotation") == "90";

                                                return new PatternPartLocation(new Point() {
                                                    X = insert.AttributeDouble("x"),
                                                    Y = insert.AttributeDouble("y")
                                                }, isRotated);
                                            }))
                        );

    }
}
