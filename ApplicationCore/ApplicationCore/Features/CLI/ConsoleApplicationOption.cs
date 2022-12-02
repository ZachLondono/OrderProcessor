using CommandLine;

namespace ApplicationCore.Features.CLI;
internal record ConsoleApplicationOption {

    [Option('c', HelpText = "Path to CSV token file")]
    public string CSVTokenFilePath { get; set; } = string.Empty;

    [Option('s', "source", Required = false, HelpText = "Source to get the order from")]
    public string Source { get; init; } = string.Empty;

    [Option('p', "provider", Required = false, HelpText = "Provider to use to load the order")]
    public string Provider { get; init; } = string.Empty;

}
