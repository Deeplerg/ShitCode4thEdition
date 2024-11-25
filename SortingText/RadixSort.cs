namespace SortingText;

public class RadixSort : ISortAlgorithm
{
    public string[]? Sort(string[]? words)
    {
        if (words == null || words.Length == 0) return words;

        int maxLength = words.Max(w => w.Length);
        for (int position = maxLength - 1; position >= 0; position--)
        {
            words = CountingSortByCharacter(words, position);
        }

        return words;
    }

    private string[]? CountingSortByCharacter(string[]? words, int position)
    {
        List<string>[] buckets = new List<string>[256];
        for (int i = 0; i < 256; i++) buckets[i] = new List<string>();

        foreach (var word in words)
        {
            char character = position < word.Length ? word[position] : '\0';
            buckets[character].Add(word);
        }

        return buckets.SelectMany(b => b).ToArray();
    }
}