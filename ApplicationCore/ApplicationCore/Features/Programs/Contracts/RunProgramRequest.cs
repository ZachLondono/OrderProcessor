using ApplicationCore.Infrastructure;
using MediatR;

namespace ApplicationCore.Features.Programs.Contracts;

public record RunProgramRequest(string ExecutablePath, string Arguments) : IQuery<RunProgramResponse>;
