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

        // Optimization: If the length difference is greater than the threshold,
        // the similarity will be below 0.3 (default threshold).
        // Since distance is at least |target.Length - query.Length|,
        // similarity = 1 - distance / maxLen <= 1 - |L1-L2| / maxLen.
        if (Math.Abs(target.Length - query.Length) > Math.Max(target.Length, query.Length) * 0.7)
            return 0.1;

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
            if (needleIdx < needle.Length && char.ToLowerInvariant(c) == needle[needleIdx])
            {
                needleIdx++;
            }
        }

        return needleIdx == needle.Length;
    }

    /// <summary>
    /// Computes Levenshtein distance between two strings with O(min(n,m)) space complexity
    /// and case-insensitive comparison. Uses stackalloc for performance on small strings.
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

        // Use stackalloc for small strings to avoid heap allocations
        // Two rows of (m + 1) integers
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

            // Swap rows
            var temp = prevRow;
            prevRow = currRow;
            currRow = temp;
        }

        return prevRow[m];
    }

    /// <summary>
    /// Filters and sorts items by fuzzy match score.
    /// Optimized to minimize allocations by avoiding LINQ and anonymous objects.
    /// </summary>
    public static IEnumerable<T> Search<T>(
        IEnumerable<T> items, 
        string query, 
        Func<T, string> textSelector, 
        double threshold = 0.3)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var item in items) yield return item;
            yield break;
        }

        // Pre-lower query once to avoid repeated allocations in the loop
        string lowerQuery = query.ToLowerInvariant();

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

        // Sort by score descending, then by index ascending to maintain stability
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
