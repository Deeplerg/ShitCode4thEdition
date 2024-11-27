namespace SortingText;

public class MergeSort : ISortAlgorithm
{
    public string[]? Sort(string[]? words)
    {
        if (words == null || words.Length <= 1) return words;

        return MergeSortRecursive(words, 0, words.Length - 1);
    }
    private string[]? MergeSortRecursive(string[] words, int left, int right)
    {
        if (left == right) return new[] { words[left] };
        int middle = (left + right) / 2;

        var leftSorted = MergeSortRecursive(words, left, middle);
        var rightSorted = MergeSortRecursive(words, middle + 1, right);

        return Merge(leftSorted, rightSorted);
    }
    private string[]? Merge(string[]? left, string[]? right)
    {
        int i = 0, j = 0, k = 0;
        string[] result = new string[left.Length + right.Length];
        while (i < left.Length && j < right.Length)
        {
            result[k++] = string.Compare(left[i], right[j], StringComparison.Ordinal) <= 0 ? left[i++] : right[j++];
        }

        while (i < left.Length)
        {
            result[k++] = left[i++];
        }

        while (j < right.Length)
        {
            result[k++] = right[j++];
        }

        return result;
    }
}