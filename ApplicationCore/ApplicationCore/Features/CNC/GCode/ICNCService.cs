using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.ReleasePDF.Contracts;

namespace ApplicationCore.Features.CNC.GCode;

public interface ICNCService
{

    Task<ReleasedJob> ExportToCNC(CNCBatch batch, IEnumerable<CNCMachineConfiguration> machineConfigs);

}
