using ApplicationCore.Features.CADCode.Services.Domain.CADCode.Configuration;

namespace ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.Configuration;

public interface ICADCodeConfigurationProvider {

    public CADCodeConfiguration GetConfiguration();

}
