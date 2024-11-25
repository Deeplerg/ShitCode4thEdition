namespace SortingText;

public class BuiltInSort : ISortAlgorithm
{
    public string[]? Sort(string[]? words)
    {
        Array.Sort(words, StringComparer.Ordinal);
        return words;
    }
}