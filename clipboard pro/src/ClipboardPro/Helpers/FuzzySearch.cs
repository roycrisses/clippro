namespace ClipboardPro.Helpers;

/// <summary>
/// Fuzzy search implementation for finding clippings
/// </summary>
public static class FuzzySearch
{
    /// <summary>
    /// Calculates similarity score between query and target string (0-1)
    /// </summary>
    /// <param name="query">The search query (MUST be pre-lowered for performance)</param>
    /// <param name="target">The string to search within</param>
    /// <param name="threshold">The minimum similarity threshold to avoid expensive calculations</param>
    public static double GetSimilarityScore(string query, string target, double threshold = 0.3)
    {
        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(target))
            return 0;

        // Exact match (case-insensitive)
        if (target.Contains(query, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        // Check if all characters of query appear in order in target
        if (ContainsInOrder(query, target))
            return 0.8;

        // Early exit: if the length difference is already greater than what's allowed by the threshold,
        // we can skip Levenshtein.
        // similarity = 1 - distance / maxLen >= threshold => distance / maxLen <= 1 - threshold
        int qLen = query.Length;
        int tLen = target.Length;
        int maxLen = Math.Max(qLen, tLen);
        int minDistance = Math.Abs(qLen - tLen);

        if ((double)minDistance / maxLen > (1.0 - threshold))
            return 0;

        // Levenshtein distance based similarity
        var distance = LevenshteinDistance(query, target, isS1Lowered: true);
        var similarity = 1.0 - (double)distance / maxLen;

        return Math.Max(0, similarity);
    }

    /// <summary>
    /// Checks if all characters in needle appear in haystack in order (case-insensitive)
    /// </summary>
    /// <param name="needle">The search query (MUST be pre-lowered for performance)</param>
    /// <param name="haystack">The string to search within</param>
    private static bool ContainsInOrder(string needle, string haystack)
    {
        if (needle.Length > haystack.Length) return false;
        
        int needleIdx = 0;
        int needleLen = needle.Length;
        int haystackLen = haystack.Length;

        for (int i = 0; i < haystackLen; i++)
        {
            // Early exit: if remaining haystack is shorter than remaining needle
            if (haystackLen - i < needleLen - needleIdx)
                return false;

            if (char.ToLowerInvariant(haystack[i]) == needle[needleIdx])
            {
                needleIdx++;
                if (needleIdx == needleLen)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Computes Levenshtein distance between two strings with O(min(n,m)) space complexity
    /// and case-insensitive comparison.
    /// </summary>
    private static int LevenshteinDistance(string s1, string s2, bool isS1Lowered = false)
    {
        // Ensure s1 is the shorter string to minimize space usage
        bool swapped = false;
        if (s1.Length > s2.Length)
        {
            (s1, s2) = (s2, s1);
            swapped = true;
        }

        int m = s1.Length;
        int n = s2.Length;

        if (m == 0) return n;

        // Pre-lower s1 if not already lowered (or if it was swapped from s2)
        string s1Lower = (isS1Lowered && !swapped) ? s1 : s1.ToLowerInvariant();

        // We only need two rows of the matrix. Use stackalloc for small strings to avoid heap allocations.
        const int StackAllocThreshold = 256;
        Span<int> prevRow = m < StackAllocThreshold ? stackalloc int[m + 1] : new int[m + 1];
        Span<int> currRow = m < StackAllocThreshold ? stackalloc int[m + 1] : new int[m + 1];

        for (int i = 0; i <= m; i++) prevRow[i] = i;

        for (int j = 1; j <= n; j++)
        {
            currRow[0] = j;
            char s2Char = char.ToLowerInvariant(s2[j - 1]);

            for (int i = 1; i <= m; i++)
            {
                int cost = s1Lower[i - 1] == s2Char ? 0 : 1;
                currRow[i] = Math.Min(
                    Math.Min(currRow[i - 1] + 1, prevRow[i] + 1),
                    prevRow[i - 1] + cost);
            }

            // Swap rows
            var temp = prevRow;
            prevRow = currRow;
            currRow = temp;
        }

        return prevRow[m];
    }

    /// <summary>
    /// Filters and sorts items by fuzzy match score
    /// </summary>
    public static IEnumerable<T> Search<T>(
        IEnumerable<T> items, 
        string query, 
        Func<T, string> textSelector, 
        double threshold = 0.3)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var item in items)
            {
                yield return item;
            }
            yield break;
        }

        // Pre-lower query once to avoid repeated allocations in the loop
        string lowerQuery = query.ToLowerInvariant();

        // Avoid LINQ for performance and to ensure stable sort (original order for same scores)
        var results = new List<(T Item, double Score, int Index)>();
        int index = 0;
        foreach (var item in items)
        {
            double score = GetSimilarityScore(lowerQuery, textSelector(item), threshold);
            if (score >= threshold)
            {
                results.Add((item, score, index));
            }
            index++;
        }

        // Sort by Score DESC, then Index ASC (stable sort)
        results.Sort((a, b) =>
        {
            int scoreComparison = b.Score.CompareTo(a.Score);
            if (scoreComparison != 0) return scoreComparison;
            return a.Index.CompareTo(b.Index);
        });

        foreach (var result in results)
        {
            yield return result.Item;
        }
    }
}
