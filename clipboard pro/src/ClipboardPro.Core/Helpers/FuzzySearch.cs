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

        // Skip expensive calculations for very long strings to maintain responsiveness
        if (query.Length > 1000 || target.Length > 1000)
            return 0;

        // Exact match (case-insensitive)
        if (target.Contains(query, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        // Check if all characters of query appear in order in target
        if (ContainsInOrder(query, target))
            return 0.8;

        // Levenshtein distance based similarity
        var distance = LevenshteinDistance(query, target, isS1Lowered: true);
        var maxLength = Math.Max(query.Length, target.Length);

        // If distance returned is -1 (threshold reached), return 0 similarity
        if (distance < 0) return 0;

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
            if (needleIdx < needle.Length && char.ToLowerInvariant(c) == needle[needleIdx])
            {
                needleIdx++;
                // Early return if we found all characters
                if (needleIdx == needle.Length) return true;
            }
        }

        return needleIdx == needle.Length;
    }

    /// <summary>
    /// Computes Levenshtein distance between two strings with O(min(n,m)) space complexity
    /// and case-insensitive comparison.
    /// </summary>
    private static int LevenshteinDistance(string s1, string s2, bool isS1Lowered = false)
    {
        // Skip expensive calculations for very long strings to maintain responsiveness
        if (s1.Length > 1000 || s2.Length > 1000)
            return -1;

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

        // Use stackalloc for small arrays to avoid heap allocations
        Span<int> prevRow = m < 256 ? stackalloc int[m + 1] : new int[m + 1];
        Span<int> currRow = m < 256 ? stackalloc int[m + 1] : new int[m + 1];

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

            // Copy current row to previous row for the next iteration
            // This is safer than reference swapping when using stackalloc/Span
            currRow.CopyTo(prevRow);
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

        // Use an explicit loop and List<(T, double, int)> to avoid LINQ and anonymous type allocations
        var results = new List<(T Item, double Score, int Index)>();
        int index = 0;
        foreach (var item in items)
        {
            double score = GetSimilarityScore(lowerQuery, textSelector(item));
            if (score >= threshold)
            {
                results.Add((item, score, index));
            }
            index++;
        }

        // Stable sort by score descending, then by original index ascending
        return results
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Index)
            .Select(x => x.Item);
    }
}
