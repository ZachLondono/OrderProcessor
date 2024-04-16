namespace ApplicationCore.Tests.Unit.Orders.Persistence.Repositories;

public abstract class RepositoryTestBase : IDisposable {

    private readonly string _filePath;

    protected TestFileOrderingConnectionFactory Factory { get; init; }

    public RepositoryTestBase() {

        _filePath = Guid.NewGuid().ToString();
        Factory = new(_filePath);

    }

    public void Dispose() {
        File.Delete(_filePath);
    }

}
