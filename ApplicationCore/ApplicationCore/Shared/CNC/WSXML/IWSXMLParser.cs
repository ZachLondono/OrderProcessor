using ApplicationCore.Shared.CNC.WSXML.Report;

namespace ApplicationCore.Shared.CNC.WSXML;
public interface IWSXMLParser {
    CNC.ReleasedJob.ReleasedJob MapDataToReleasedJob(WSXMLReport report, DateTime orderDate, DateTime? dueDate, string customerName, string vendorName);
}