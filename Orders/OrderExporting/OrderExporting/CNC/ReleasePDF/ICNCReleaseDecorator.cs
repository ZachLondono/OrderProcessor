using OrderExporting.CNC.Programs.Job;
using OrderExporting.Shared;

namespace OrderExporting.CNC.ReleasePDF;

public interface ICNCReleaseDecorator : IDocumentDecorator {

    public void AddData(ReleasedJob job);

    //public void AddData(ReleasedJob job);

}
