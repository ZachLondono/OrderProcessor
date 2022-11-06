using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Contracts;

internal record CNCReleaseRequest(CNCBatch Batch) : ICommand;
