namespace ApplicationCore.Features.CNC.GCode.Configuration;

public interface ICADCodeMachineConfigurationProvider
{

    public IEnumerable<CADCodeMachineConfiguration> GetConfigurations();

}