using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Options;
using ApplicationCore.Features.CNC.GCode.Contracts.Results;
using ApplicationCore.Features.CNC.GCode.Domain.CADCode;
using ApplicationCore.Features.CNC.GCode.Services;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.CNC.GCode;

public class GenerateGCode {

    public record Command(Batch Batch, GCodeGenerationOptions Options) : ICommand<GCodeGenerationResult>;

    public class Handler : CommandHandler<Command, GCodeGenerationResult> {

        private readonly GCodeGenerator _generator;

        public Handler(GCodeGenerator generator) {
            _generator = generator;
        }

        public override async Task<Response<GCodeGenerationResult>> Handle(Command command) {
            try {

                var result = await _generator.GenerateGCodeAsync(command.Batch, command.Options);

                return new(result);

            } catch (CADCodeFailedToInitilizeException) {
                return new Response<GCodeGenerationResult>(new Error() {
                    Title = "Could not start CADCode",
                    Details = "Missing CADCode license key or CADCode failed while trying to initilize"
                });
            } catch (Exception e) {
                return new Response<GCodeGenerationResult>(new Error() {
                    Title = "Exception thrown while releasing cnc program",
                    Details = e.ToString()
                });
            }
        }
    }

}
