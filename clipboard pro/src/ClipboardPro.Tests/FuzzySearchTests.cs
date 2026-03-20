using ClipboardPro.Helpers;
using Xunit;

namespace ClipboardPro.Tests;

public class FuzzySearchTests
{
    [Theory]
    [InlineData("test", "test", 1.0)]
    [InlineData("TEST", "test", 1.0)]
    [InlineData("test", "TEST", 1.0)]
    [InlineData("abc", "axbycz", 0.8)]
    [InlineData("abc", "abcde", 1.0)] // Contains
    [InlineData("apple", "apply", 0.8)] // Levenshtein similarity
    [InlineData("kitten", "sitting", 0.5)] // Levenshtein similarity (different lengths)
    public void GetSimilarityScore_ReturnsExpectedScore(string query, string target, double expectedMinScore)
    {
        // Pre-lower query as per requirement
        var score = FuzzySearch.GetSimilarityScore(query.ToLowerInvariant(), target);

        Assert.True(score >= expectedMinScore, $"Expected score for '{query}' in '{target}' to be >= {expectedMinScore}, but was {score}");
    }

    [Fact]
    public void GetSimilarityScore_ThresholdOptimization_ReturnsZeroForLargeStrings()
    {
        var query = "test";
        var largeTarget = new string('a', 1001);

        var score = FuzzySearch.GetSimilarityScore(query, largeTarget);

        Assert.Equal(0, score);
    }

    [Fact]
    public void Search_ReturnsOrderedResults()
    {
        var items = new List<string> { "apple", "apply", "banana", "apricot" };
        var query = "appl";

        var results = FuzzySearch.Search(items, query, s => s).ToList();

        Assert.Equal("apple", results[0]);
        Assert.Equal("apply", results[1]);
        Assert.DoesNotContain("banana", results);
    }
}
