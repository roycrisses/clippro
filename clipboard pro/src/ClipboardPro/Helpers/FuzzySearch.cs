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

        // Optimization: skip expensive Levenshtein for very large strings (e.g. large clipboard data)
        if (target.Length > 1000)
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
            if (needleIdx == needle.Length) return true;

            // Compare char of haystack (lowered) with already lowered needle char
            if (char.ToLowerInvariant(c) == needle[needleIdx])
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
        bool s1WasLowered = isS1Lowered;
        if (s1.Length > s2.Length)
        {
            (s1, s2) = (s2, s1);
            s1WasLowered = false; // s1 is now the original s2, which isn't pre-lowered
        }

        int m = s1.Length;
        int n = s2.Length;

        if (m == 0) return n;

        // Use stackalloc for small strings to avoid heap allocations
        Span<int> prevRow = m < 256 ? stackalloc int[m + 1] : new int[m + 1];
        Span<int> currRow = m < 256 ? stackalloc int[m + 1] : new int[m + 1];

        // Pre-lower the shorter string once to avoid repeated char.ToLowerInvariant calls in the inner loop
        Span<char> s1Lowered = m < 256 ? stackalloc char[m] : new char[m];
        for (int i = 0; i < m; i++)
        {
            s1Lowered[i] = s1WasLowered ? s1[i] : char.ToLowerInvariant(s1[i]);
        }

        for (int i = 0; i <= m; i++) prevRow[i] = i;

        for (int j = 1; j <= n; j++)
        {
            currRow[0] = j;
            char s2Char = char.ToLowerInvariant(s2[j - 1]);

            for (int i = 1; i <= m; i++)
            {
                int cost = s1Lowered[i - 1] == s2Char ? 0 : 1;

                int insert = currRow[i - 1] + 1;
                int delete = prevRow[i] + 1;
                int substitute = prevRow[i - 1] + cost;

                // Inline Math.Min for performance
                int min = insert < delete ? insert : delete;
                currRow[i] = min < substitute ? min : substitute;
            }

            // Copy currRow to prevRow to avoid Span swapping issues in .NET
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
        {
            foreach (var item in items)
                yield return item;
            yield break;
        }

        // Pre-lower query once to avoid repeated allocations in the loop
        string lowerQuery = query.ToLowerInvariant();

        // Use a list of ValueTuples to avoid anonymous type allocations (heap pressure)
        // We also store the original index to ensure a stable sort (Enumerable.OrderBy is stable,
        // but List<T>.Sort is NOT, and we want to avoid extra Linq overhead)
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

        // Sort by score descending, then by index ascending (stable)
        scoredItems.Sort((a, b) =>
        {
            int scoreComparison = b.Score.CompareTo(a.Score);
            if (scoreComparison != 0) return scoreComparison;
            return a.Index.CompareTo(b.Index);
        });

        foreach (var scoredItem in scoredItems)
        {
            yield return scoredItem.Item;
        }
    }
}
