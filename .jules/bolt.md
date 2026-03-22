## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-22 - [FuzzySearch Allocation & Stability Optimization]
**Learning:** LINQ chains with anonymous types in a tight search loop (e.g., `Search<T>`) create significant heap pressure (93KB per search for 500 items). `List<T>.Sort` is unstable, which can disrupt the chronological order of results with identical similarity scores.
**Action:** Replace LINQ with a manual `foreach` loop and `ValueTuple` (~100x allocation reduction). Use an index-augmented comparison to ensure a stable sort while maintaining high performance.
