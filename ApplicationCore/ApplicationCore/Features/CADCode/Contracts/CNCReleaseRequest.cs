using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Contracts;

public record CNCReleaseRequest(CNCBatch Batch) : ICommand;
