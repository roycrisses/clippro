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
    public static double GetSimilarityScore(string query, string target)
    {
        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(target))
            return 0;

        // Exact match (case-insensitive)
        if (target.Contains(query, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        // Check if all characters of query appear in order in target
        if (ContainsInOrder(query, target))
            return 0.8;

        // If target is much longer than query, Levenshtein is expensive and unlikely to yield a useful score
        // We cap it at 1000 characters to prevent performance issues with huge clipboard contents
        if (target.Length > 1000 && !target.Contains(query, StringComparison.OrdinalIgnoreCase))
            return 0;

        // Levenshtein distance based similarity
        var distance = LevenshteinDistance(query, target, isS1Lowered: true);
        var maxLength = Math.Max(query.Length, target.Length);
        var similarity = 1.0 - (double)distance / maxLength;

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

        return needleIdx == needle.Length;
    }

    /// <summary>
    /// Computes Levenshtein distance between two strings with O(min(n,m)) space complexity
    /// and case-insensitive comparison using stackalloc for small strings.
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

        // Use stackalloc for small strings to avoid heap allocations
        // 256 is a safe limit for stackalloc in most environments
        Span<int> prevRow = m <= 256 ? stackalloc int[m + 1] : new int[m + 1];
        Span<int> currRow = m <= 256 ? stackalloc int[m + 1] : new int[m + 1];

        for (int i = 0; i <= m; i++) prevRow[i] = i;

        for (int j = 1; j <= n; j++)
        {
            currRow[0] = j;
            char s2Char = char.ToLowerInvariant(s2[j - 1]);

            for (int i = 1; i <= m; i++)
            {
                // Compare characters directly to avoid string allocations from ToLowerInvariant()
                char s1Char = (isS1Lowered && !swapped) ? s1[i - 1] : char.ToLowerInvariant(s1[i - 1]);
                int cost = s1Char == s2Char ? 0 : 1;

                // Inline Math.Min for performance in hot loop
                int minInsertDelete = currRow[i - 1] < prevRow[i] ? currRow[i - 1] + 1 : prevRow[i] + 1;
                int substitution = prevRow[i - 1] + cost;
                currRow[i] = minInsertDelete < substitution ? minInsertDelete : substitution;
            }

            // Swap rows
            var temp = prevRow;
            prevRow = currRow;
            currRow = temp;
        }

        return prevRow[m];
    }

    /// <summary>
    /// Filters and sorts items by fuzzy match score.
    /// Uses ValueTuple and explicit loops to minimize heap allocations and ensure stable sorting.
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

        // Use a list of ValueTuples to avoid anonymous object allocations
        // We include the original index to maintain a stable sort (Enumerable.OrderBy is stable,
        // but we want to be explicit and efficient).
        var scoredItems = new List<(T Item, double Score, int Index)>();
        int index = 0;

        foreach (var item in items)
        {
            double score = GetSimilarityScore(lowerQuery, textSelector(item));
            if (score >= threshold)
            {
                scoredItems.Add((item, score, index));
            }
            index++;
        }

        // Sort by score descending, then by original index to ensure stability
        return scoredItems
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Index)
            .Select(x => x.Item);
    }
}
