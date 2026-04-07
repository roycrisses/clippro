## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-15 - [FuzzySearch Performance and Memory Optimization]
**Learning:** LINQ pipelines like `Select(...).Where(...).OrderByDescending(...)` on collection of items for search cause significant allocations (approx. 117KB for 500 items) due to anonymous objects and iterator overhead. Additionally, calling `LevenshteinDistance` on very large clipboard strings (>1000 chars) is a performance killer for UI responsiveness.
**Action:** Replace LINQ with manual loops and `List<(T, double, int)>` for stable sorting with minimal allocations. Implement a character threshold to skip expensive distance calculations for large strings. Use `stackalloc Span<int>` for DP matrix rows to eliminate heap allocations for common search strings (<256 chars).
