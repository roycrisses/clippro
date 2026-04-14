## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2026-04-14 - [FuzzySearch Performance and Memory Optimization]
**Learning:** LINQ chains (`Select`, `Where`, `OrderBy`) on hot paths like search result filtering create significant GC pressure due to anonymous type and iterator allocations. Additionally, Levenshtein distance can be mathematically bypassed for obviously poor matches using a simple length-ratio check.
**Action:** Replace LINQ with manual loops and `List.Sort` with index-augmented tuples for stable sorting. Use `stackalloc` for small distance matrices and implement a length-ratio heuristic to skip expensive calculations.
