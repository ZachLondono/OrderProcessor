using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.ProgramRelease;
using ApplicationCore.Features.CNC.GCode.Domain;

namespace ApplicationCore.Features.CNC.GCode;

public interface ICNCService
{

    Task<ReleasedJob> ExportToCNC(CNCBatch batch, IEnumerable<CNCMachineConfiguration> machineConfigs);

}
