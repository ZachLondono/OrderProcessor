using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Contracts.ProgramRelease;
using ApplicationCore.Features.CADCode.Services.Domain;

namespace ApplicationCore.Features.CADCode.Services;

public interface ICNCService {

    ReleasedJob ExportToCNC(CNCBatch batch, IEnumerable<CNCMachineConfiguration> machineConfigs);

}
