using ApplicationCore.Shared;
using System.Xml.Linq;

namespace ApplicationCore.Features.CNC.ReleasePDF.WSXML;

internal partial class WSXMLParser {

    public static WSXMLReport? ParseWSXMLReport(string reportFilePath) {

        XDocument xdoc = XDocument.Load(reportFilePath);
        if (xdoc.Root is null) {
            Console.WriteLine("No root found");
            return null;
        }

        var timestamp = new FileInfo(reportFilePath).LastWriteTime;

        var job = xdoc.Root.Element("Job");
        if (job is null) {
            Console.WriteLine("No job found");
            return null;
        }

        var manufacturing = job.Element("Manufacturing");
        if (manufacturing is null) {
            Console.WriteLine("No manufacturing data found");
            return null;
        }

        var nestedParts = job.Elements("Item")
                           .Where(item => item.ElementValue("Note") == "Nested blocknest")
                           .Where(nest => nest.Elements("Part").Any())
                           .SelectMany(item =>
                                item.Elements("Part")
                                            .Select(Part.FromXElemnt)
                                            .ToList())
                           .DistinctBy(part => part.Id)
                           .ToDictionary(part => part.Id, part => part);

        var singleParts = job.Elements("Item")
                           .Where(item => item.ElementValue("Note") == "Single Program Code")
                           .Where(nest => nest.Elements("Part").Any())
                           .SelectMany(item =>
                                item.Elements("Part")
                                            .Select(Part.FromXElemnt)
                                            .ToList())
                           .DistinctBy(part => part.Id)
                           .ToDictionary(part => part.Id, part => part);

        var allParts = new Dictionary<string, Part>();
        nestedParts.ForEach(kv => allParts[kv.Key] = kv.Value);
        singleParts.ForEach(kv => allParts[kv.Key] = kv.Value);

        var patternScheduleItems = manufacturing.Elements("PatternSchedule");

        var patternSchedules = patternScheduleItems.Select(PatternSchedule.FromXElement);

        var items = job.Elements("Item").Select(Item.FromXElement);

        var materials = job.Elements("Material")
                            .Select(MaterialRecord.FromXElement)
                            .ToDictionary(m => m.Id);

        var labels = manufacturing.Elements("Label")
                                    .Select(PartLabels.FromXElement)
                                    .ToDictionary(l => l.Id);

        var operationGroups = manufacturing.Elements("OperationGroups")
                                            .Select(OperationGroups.FromXElement);

        var report = new WSXMLReport() {
            JobName = job.ElementValue("Name"),
            Parts = allParts,
            PatternSchedules = patternSchedules,
            Items = items,
            Materials = materials,
            PartLabels = labels,
            OperationGroups = operationGroups,
            TimeStamp = timestamp
        };

        return report;

    }

}
