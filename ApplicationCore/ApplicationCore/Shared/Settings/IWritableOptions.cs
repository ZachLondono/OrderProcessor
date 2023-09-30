using Microsoft.Extensions.Options;

namespace ApplicationCore.Shared.Settings;

public interface IWritableOptions<out T> : IOptionsSnapshot<T> where T : class, new() {
    void Update(Action<T> applyChanges);
}
