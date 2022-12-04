using ApplicationCore.Features.CNC.Services.Domain.CADCode.Configuration;

namespace ApplicationCore.Features.CNC.Services.Services.CADCodeGCode.Configuration;

public interface ICADCodeConfigurationProvider {

    public CADCodeConfiguration GetConfiguration();

}
