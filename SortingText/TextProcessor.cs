﻿namespace SortingText;

public class TextProcessor
{
    private readonly ISortAlgorithm _sortAlgorithm;

    public TextProcessor(ISortAlgorithm sortAlgorithm)
    {
        _sortAlgorithm = sortAlgorithm;
    }
    
    public string[]? SplitText(string text)
    {
        return text.Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    }
    
    public string[]? SortWords(string[]? words)
    {
        return _sortAlgorithm.Sort(words);
    }
    
    public Dictionary<string, int> CountWords(string[]? words)
    {
        return words.GroupBy(word => word)
            .ToDictionary(group => group.Key, group => group.Count());
    }
}