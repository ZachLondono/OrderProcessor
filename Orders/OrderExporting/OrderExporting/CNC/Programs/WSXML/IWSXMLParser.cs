using OrderExporting.CNC.Programs.Job;
using OrderExporting.CNC.Programs.WSXML.Report;

namespace OrderExporting.CNC.Programs.WSXML;

public interface IWSXMLParser {
    ReleasedJob MapDataToReleasedJob(WSXMLReport report, DateTime orderDate, DateTime? dueDate, string customerName, string vendorName);
}