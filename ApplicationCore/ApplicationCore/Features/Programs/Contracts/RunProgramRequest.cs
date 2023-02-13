using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Programs.Contracts;

public record RunProgramRequest(string ExecutablePath, string Arguments) : ICommand<RunProgramResponse>;
