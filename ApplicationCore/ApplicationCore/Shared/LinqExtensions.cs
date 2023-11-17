namespace ApplicationCore.Shared;

internal static class LinqExtensions {

    public static void ForEach<T>(this IEnumerable<T> values, Action<T> action) {
        foreach (var value in values)
            action(value);
    }

    public static void ForEach<T>(this IEnumerable<T> values, Action<T, int> action) {
        int index = 0;
        foreach (var value in values)
            action(value, index++);
    }

    public static string GetValueOrEmpty(this IDictionary<string, string> dict, string key) {
        if (dict.TryGetValue(key, out string? value)) return value;
        return string.Empty;
    }

}