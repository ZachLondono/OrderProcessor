using ApplicationCore.Features.CADCode.Contracts.ProgramRelease;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Contracts;

// TODO: rename to generate gcode request
public record CNCReleaseRequest(CNCBatch Batch) : ICommand<ReleasedJob>;
