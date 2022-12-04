using ApplicationCore.Features.CNC.ReleasePDF.Contracts;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.GCode.Contracts;

// TODO: rename to generate gcode request
public record CNCReleaseRequest(CNCBatch Batch) : ICommand<ReleasedJob>;
