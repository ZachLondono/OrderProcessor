using System.Xml.Linq;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.WSXML;

internal record Part(string Id, string LabelId, string Name, double Width, double Length, IEnumerable<string> PatternScheduleIds) {
    public static Part FromXElemnt(XElement element) => new(element.AttributeValue("ID"), element.AttributeValue("LabelID"), element.ElementValue("Name"), element.ElementDouble("FinishedLength"), element.ElementDouble("FinishedWidth"), element.AttributeValue("PatSchID").Split(' '));
}
