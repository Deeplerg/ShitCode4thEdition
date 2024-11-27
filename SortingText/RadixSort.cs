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
        var buckets = new Dictionary<char, List<string>>();

        foreach (var word in words)
        {
            char character = position < word.Length ? word[position] : '\0';
            if (!buckets.ContainsKey(character))
            {
                buckets[character] = new List<string>();
            }
            buckets[character].Add(word);
        }

        var sortedWords = new List<string>();
        for (int i = 0; i <= char.MaxValue; i++)
        {
            char currentChar = (char)i;
            if (buckets.TryGetValue(currentChar, out var bucket))
            {
                sortedWords.AddRange(bucket);
            }
        }

        return sortedWords.ToArray();
    }
}