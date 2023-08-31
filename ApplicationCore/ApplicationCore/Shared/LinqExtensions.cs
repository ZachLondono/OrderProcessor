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

}