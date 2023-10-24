using System.Xml.Linq;
using ApplicationCore.Shared.CNC.WSXML;

namespace ApplicationCore.Shared.CNC.WSXML.Report;

public record OperationGroups(string Id, string JobId, string? PartId, string? MfgOrientationId, IEnumerable<string> ToolName) {

    public static OperationGroups FromXElement(XElement element) {

        // An operation groups element should only contain either a `PartId`, if it is a single part program, or a `MfgOrientationId` if it is a nested part program
        string? partId = null;
        string? mfgOrientationId = null;
        foreach (var attr in element.Attributes()) {

            if (attr.Name.LocalName == "PartID") {
                partId = attr.Value;
                break;
            } else if (attr.Name.LocalName == "MfgOrientationID") {
                mfgOrientationId = attr.Value;
                break;
            }

        }

        // `ToolName` elements may contain a single tool name or multiple comma seperated names
        var toolNames = element.Elements("ToolName").SelectMany(e => e.Value.Split(','));

        return new(element.AttributeValue("ID"), element.AttributeValue("JobId"), partId, mfgOrientationId, toolNames);

    }

};
