namespace Domain.Infrastructure.Data;

public class IncompatibleDatabaseVersion : Exception {
    public int FoundVersion { get; set; }
    public int ExpectedVersion { get; set; }
    public IncompatibleDatabaseVersion(int foundVersion, int expectedVersion) : base($"Database version is {foundVersion}, but {expectedVersion} is required") {
        FoundVersion = foundVersion;
        ExpectedVersion = expectedVersion;
    }
}
