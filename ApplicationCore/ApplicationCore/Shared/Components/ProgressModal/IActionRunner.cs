namespace ApplicationCore.Features.Shared.Components.ProgressModal;

public interface IActionRunner {

    public Action<ProgressLogMessage>? PublishProgressMessage { get; set; }

    public Task Run();

}
