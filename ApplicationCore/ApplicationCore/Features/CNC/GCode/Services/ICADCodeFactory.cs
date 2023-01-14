namespace ApplicationCore.Features.CNC.GCode.Services;

public interface ICADCodeFactory {

    // Loads configuration to initlize cadcode
    ICADCodeProxy CreateCADCode();

}
