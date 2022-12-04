using ApplicationCore.Features.CNC.Contracts.ProgramRelease;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.Contracts;

// TODO: rename to generate gcode request
public record CNCReleaseRequest(CNCBatch Batch) : ICommand<ReleasedJob>;
