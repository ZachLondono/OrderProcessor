namespace ApplicationCore.Features.CNC.ReleasePDF.WSXML;

internal class WSXMLReport {

    public required string JobName { get; set; }
    public required Dictionary<string, Part> Parts { get; set; }
    public required IEnumerable<PatternSchedule> PatternSchedules { get; set; }
    public required IEnumerable<Item> Items { get; set; }
    public required Dictionary<string, MaterialRecord> Materials { get; set; }
    public required Dictionary<string, PartLabels> PartLabels { get; set; }
    public required IEnumerable<OperationGroups> OperationGroups { get; set; }

}
