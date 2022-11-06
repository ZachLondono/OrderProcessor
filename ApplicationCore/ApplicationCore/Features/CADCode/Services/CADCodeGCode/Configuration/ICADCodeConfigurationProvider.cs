using ApplicationCore.Features.CADCode.Services.Domain.CADCode.Configuration;

namespace ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode.Configuration;

internal interface ICADCodeConfigurationProvider {

    public CADCodeConfiguration GetConfiguration();

}
