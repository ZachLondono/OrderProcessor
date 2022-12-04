using ApplicationCore.Features.CNC.GCode.Contracts.ProgramRelease;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.GCode.Contracts;

// TODO: rename to generate gcode request
public record CNCReleaseRequest(CNCBatch Batch) : ICommand<ReleasedJob>;
