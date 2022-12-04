using ApplicationCore.Features.CNC.GCode.Domain.CADCode.Configuration;

namespace ApplicationCore.Features.CNC.GCode.Configuration;

public interface ICADCodeConfigurationProvider
{

    public CADCodeConfiguration GetConfiguration();

}
