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

        // Optimization: if length difference is too high, it can't possibly be a good match.
        // Similarity threshold is 0.3, so distance must be <= 0.7 * maxLength.
        // Since distance is at least the length difference, we can skip if diff > 0.7 * maxLength.
        int maxLength = Math.Max(query.Length, target.Length);
        int diff = Math.Abs(query.Length - target.Length);
        if (diff > maxLength * 0.7)
            return 0;

        // Levenshtein distance based similarity
        var distance = LevenshteinDistance(query, target, isS1Lowered: true);
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

                // Optimized Math.Min for 3 values
                int min = currRow[i - 1] + 1;
                if (prevRow[i] + 1 < min) min = prevRow[i] + 1;
                if (prevRow[i - 1] + cost < min) min = prevRow[i - 1] + cost;
                currRow[i] = min;
            }

            // Swap rows
            var temp = prevRow;
            prevRow = currRow;
            currRow = temp;
        }

        return prevRow[m];
    }

    /// <summary>
    /// Filters and sorts items by fuzzy match score. Use manual loop and stable sort to reduce allocations.
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

        // Use a manual loop to filter items, reducing allocations from LINQ.
        // Also capture the original index to ensure a stable sort when scores are equal.
        // Pre-allocate the list if we can determine the capacity to avoid resizing allocations.
        int capacity = items is System.Collections.ICollection collection ? collection.Count : 0;
        var results = new List<(T Item, double Score, int Index)>(capacity);

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

        // Stable sort: Sort by Score DESC, then by original Index ASC
        results.Sort((a, b) =>
        {
            int scoreCompare = b.Score.CompareTo(a.Score);
            if (scoreCompare != 0) return scoreCompare;
            return a.Index.CompareTo(b.Index);
        });

        foreach (var result in results)
        {
            yield return result.Item;
        }
    }
}
