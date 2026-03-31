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
    /// <param name="threshold">The minimum similarity score to consider (optional)</param>
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

        int m = query.Length;
        int n = target.Length;
        int maxLen = Math.Max(m, n);

        // Fast exit: if length difference is already too great to meet threshold
        if (1.0 - (double)Math.Abs(m - n) / maxLen < threshold)
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
        foreach (char c in haystack)
        {
            // Compare char of haystack (lowered) with already lowered needle char
            if (char.ToLowerInvariant(c) == needle[needleIdx])
            {
                needleIdx++;
                if (needleIdx == needle.Length) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Computes Levenshtein distance between two strings with O(min(n,m)) space complexity
    /// and case-insensitive comparison. Uses stackalloc for small strings to avoid allocations.
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

        // Use stackalloc for small strings (< 256 chars) to avoid heap allocations and GC pressure
        Span<int> row1 = m < 256 ? stackalloc int[m + 1] : new int[m + 1];
        Span<int> row2 = m < 256 ? stackalloc int[m + 1] : new int[m + 1];

        Span<int> prevRow = row1;
        Span<int> currRow = row2;

        for (int i = 0; i <= m; i++) prevRow[i] = i;

        bool useS1Directly = isS1Lowered && !swapped;

        for (int j = 1; j <= n; j++)
        {
            currRow[0] = j;
            char s2Char = char.ToLowerInvariant(s2[j - 1]);

            for (int i = 1; i <= m; i++)
            {
                char s1Char = useS1Directly ? s1[i - 1] : char.ToLowerInvariant(s1[i - 1]);
                int cost = s1Char == s2Char ? 0 : 1;

                // Optimized Math.Min for performance
                int min = currRow[i - 1] + 1;
                if (prevRow[i] + 1 < min) min = prevRow[i] + 1;
                if (prevRow[i - 1] + cost < min) min = prevRow[i - 1] + cost;
                currRow[i] = min;
            }

            // Swap spans (efficient row swapping)
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
            return items;

        // Pre-lower query once to avoid repeated allocations in the loop
        string lowerQuery = query.ToLowerInvariant();

        // Use manual loop and ValueTuple to avoid LINQ/anonymous type allocations
        var results = new List<(T Item, double Score)>();

        foreach (var item in items)
        {
            double score = GetSimilarityScore(lowerQuery, textSelector(item), threshold);
            if (score >= threshold)
            {
                results.Add((item, score));
            }
        }

        return results
            .OrderByDescending(x => x.Score)
            .Select(x => x.Item);
    }
}
