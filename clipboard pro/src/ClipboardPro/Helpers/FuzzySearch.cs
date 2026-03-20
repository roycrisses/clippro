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

        // Optimization: For very large strings (e.g., massive clipboard items),
        // skip expensive O(N*M) Levenshtein and O(N) ContainsInOrder to keep UI responsive.
        if (target.Length > 1000)
            return 0;

        // Check if all characters of query appear in order in target
        if (ContainsInOrder(query, target))
            return 0.8;

        // Levenshtein distance based similarity
        var distance = LevenshteinDistance(query, target, isS1Lowered: true);
        var maxLength = Math.Max(query.Length, target.Length);
        var similarity = 1.0 - (double)distance / maxLength;

        return Math.Max(0, similarity);
    }

    /// <summary>
    /// Checks if all characters in needle appear in haystack in order (case-insensitive).
    /// Uses ReadOnlySpan for performance and exits early once all characters are found.
    /// </summary>
    private static bool ContainsInOrder(ReadOnlySpan<char> needle, ReadOnlySpan<char> haystack)
    {
        if (needle.Length > haystack.Length) return false;
        if (needle.IsEmpty) return true;

        int needleIdx = 0;
        foreach (char c in haystack)
        {
            // Compare char of haystack (lowered) with already lowered needle char
            if (char.ToLowerInvariant(c) == needle[needleIdx])
            {
                needleIdx++;
                // Early exit: once we found all characters in order, we're done.
                if (needleIdx == needle.Length)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Computes Levenshtein distance between two strings with O(min(n,m)) space complexity
    /// and case-insensitive comparison. Uses stackalloc for row buffers when small.
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

        int m = s1.Length; // shorter length
        int n = s2.Length; // longer length

        if (m == 0) return n;

        // Pre-lower s1 if not already lowered (or if it was swapped from s2)
        string s1Lower = (isS1Lowered && !swapped) ? s1 : s1.ToLowerInvariant();

        // Optimized path for small strings (common in search queries)
        // Uses stackalloc to avoid heap allocation for row buffers.
        if (m <= 256)
        {
            Span<int> prevRow = stackalloc int[m + 1];
            Span<int> currRow = stackalloc int[m + 1];
            return LevenshteinDistanceInternal(s1Lower.AsSpan(), s2.AsSpan(), prevRow, currRow);
        }

        // Fallback to heap for large strings
        int[] prevRowArr = new int[m + 1];
        int[] currRowArr = new int[m + 1];
        return LevenshteinDistanceInternal(s1Lower.AsSpan(), s2.AsSpan(), prevRowArr, currRowArr);
    }

    /// <summary>
    /// Internal Levenshtein implementation that works with spans for buffers and text.
    /// Performs O(1) buffer swapping instead of CopyTo.
    /// </summary>
    private static int LevenshteinDistanceInternal(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2, Span<int> prevRow, Span<int> currRow)
    {
        int m = s1.Length; // shorter length
        int n = s2.Length; // longer length

        for (int i = 0; i <= m; i++) prevRow[i] = i;

        for (int j = 1; j <= n; j++)
        {
            currRow[0] = j;
            char s2Char = char.ToLowerInvariant(s2[j - 1]);

            for (int i = 1; i <= m; i++)
            {
                int cost = s1[i - 1] == s2Char ? 0 : 1;
                currRow[i] = Math.Min(
                    Math.Min(currRow[i - 1] + 1, prevRow[i] + 1),
                    prevRow[i - 1] + cost);
            }

            // Swap Span pointers (O(1) operation)
            var temp = prevRow;
            prevRow = currRow;
            currRow = temp;
        }

        return prevRow[m];
    }

    /// <summary>
    /// Filters and sorts items by fuzzy match score.
    /// Uses ValueTuple and explicit loops to avoid LINQ/anonymous object heap allocations.
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

        // Use List with ValueTuples to avoid anonymous object allocations
        var scoredItems = new List<(T Item, double Score)>();
        foreach (var item in items)
        {
            double score = GetSimilarityScore(lowerQuery, textSelector(item));
            if (score >= threshold)
            {
                scoredItems.Add((item, score));
            }
        }

        // Sort by score descending and return only items
        return scoredItems
            .OrderByDescending(x => x.Score)
            .Select(x => x.Item);
    }
}
