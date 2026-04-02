## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2026-01-16 - [FuzzySearch Performance & Allocations]
**Learning:** LINQ in hot paths (like search-as-you-type) causes significant allocations of anonymous objects and iterators. Additionally, running Levenshtein on very large clipboard content (e.g. log files) is unnecessary if simpler checks like `Contains` fail or if the length difference is too large to ever satisfy the similarity threshold.
**Action:** Replace LINQ with manual loops and `List.Sort` with index-augmented `ValueTuple` for stable sorting. Add early exits for length-difference thresholds and skip expensive fuzzy matching for strings over 1000 characters. Use `stackalloc` for small distances.
