using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.CNC.Contracts.ProgramRelease;
using ApplicationCore.Features.CNC.Services.Domain;

namespace ApplicationCore.Features.CNC.Services;

public interface ICNCService {

    Task<ReleasedJob> ExportToCNC(CNCBatch batch, IEnumerable<CNCMachineConfiguration> machineConfigs);

}
