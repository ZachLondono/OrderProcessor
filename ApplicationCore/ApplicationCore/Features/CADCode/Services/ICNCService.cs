using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Services.Domain;

namespace ApplicationCore.Features.CADCode.Services;

public interface ICNCService {

    void ExportToCNC(CNCBatch batch, IEnumerable<CNCMachineConfiguration> machineConfigs);

}
