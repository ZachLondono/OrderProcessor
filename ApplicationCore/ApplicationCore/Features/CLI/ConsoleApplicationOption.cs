using CommandLine;

namespace ApplicationCore.Features.CLI;
internal record ConsoleApplicationOption {

    [Option('s', "source", Required = true, HelpText = "Source to get the order from")]
    public string Source { get; init; } = string.Empty;

    [Option('p', "provider", Required = true, HelpText = "Provider to use to load the order")]
    public string Provider { get; init; } = string.Empty;

}
