namespace Graphs.Extensions;

public static class ListExtensions
{
    public static T GetRandomEntry<T>(
        this ICollection<T> values,
        Random? random,
        List<T>? exclude = null
    )
    {
        random ??= new Random();
        exclude ??= [];

        var filteredValues = values.Except(exclude).ToList();
        if (filteredValues.Count == 0)
            throw new ArgumentException("No filtered values to choose from.");

        return filteredValues[random.Next(0, values.Count)];
    }

    public static T RemoveRandomEntry<T>(
        this ICollection<T> values,
        Random? random,
        List<T>? exclude = null
    )
    {
        var value = GetRandomEntry(values, random, exclude);
        values.Remove(value);
        return value;
    }
}
