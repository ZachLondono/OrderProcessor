using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Services.Domain;
using ApplicationCore.Features.CADCode.Services.Domain.ProgramRelease;

namespace ApplicationCore.Features.CADCode.Services;

public interface ICNCService {

    ReleasedJob ExportToCNC(CNCBatch batch, IEnumerable<CNCMachineConfiguration> machineConfigs);

}
