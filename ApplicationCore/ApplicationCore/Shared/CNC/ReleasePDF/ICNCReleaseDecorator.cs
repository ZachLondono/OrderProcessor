using ApplicationCore.Shared;
using ApplicationCore.Shared.CNC.WSXML.ReleasedJob;

namespace ApplicationCore.Shared.CNC.ReleasePDF;

public interface ICNCReleaseDecorator : IDocumentDecorator {

    public void AddData(ReleasedJob job);

    //public void AddData(ReleasedJob job);

}
