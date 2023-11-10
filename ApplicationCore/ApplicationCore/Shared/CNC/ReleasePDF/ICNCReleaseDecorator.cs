using ApplicationCore.Shared.CNC.Job;

namespace ApplicationCore.Shared.CNC.ReleasePDF;

public interface ICNCReleaseDecorator : IDocumentDecorator {

    public void AddData(ReleasedJob job);

    //public void AddData(ReleasedJob job);

}
