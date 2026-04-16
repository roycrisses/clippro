## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2026-04-16 - [FuzzySearch Allocation and Heuristic Optimization]
**Learning:** LINQ operations in tight loops (like search filtering) can cause significant GC pressure due to anonymous object allocations. Additionally, performing Levenshtein distance on strings with wildly different lengths is wasteful if a similarity threshold is required.
**Action:** Replace LINQ with manual loops and List<(T, double, int)> to reduce allocations and allow stable sorting. Implement a length-ratio heuristic to early-exit when the maximum possible similarity cannot meet the threshold. Use stackalloc for small Span<int> buffers to avoid heap allocations in hot paths.
