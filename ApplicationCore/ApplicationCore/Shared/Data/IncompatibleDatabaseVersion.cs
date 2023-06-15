using ApplicationCore.Shared.Data.Ordering;

namespace ApplicationCore.Shared.Data;

public class IncompatibleDatabaseVersion : Exception {
    public int FoundVersion { get; set; }
    public IncompatibleDatabaseVersion(int foundVersion) : base($"Database version is {foundVersion}, but {SqliteOrderingDbConnectionFactory.DB_VERSION} is required") {
        FoundVersion = foundVersion;
    }
}