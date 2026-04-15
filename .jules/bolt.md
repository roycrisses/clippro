## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-20 - [Fuzzy Search Heuristics & Allocation Reduction]
**Learning:** Fuzzy search performance in large lists is dominated by string allocations and unnecessary Levenshtein calculations. A length-ratio check `Math.Abs(L1-L2) / Max(L1,L2) > (1 - threshold)` can skip expensive calculations for matches that mathematically cannot meet the threshold. LINQ chains (`Select`, `Where`, `OrderBy`) create significant iterator overhead and anonymous object allocations; manual loops with `List<(T, double, int)>.Sort` are much faster and maintain stable sorting.
**Action:** Implement mathematical early exits before O(N*M) algorithms and prefer manual loops over LINQ in search hot paths to minimize GC pressure and improve throughput.
