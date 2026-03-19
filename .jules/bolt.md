## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2026-05-20 - [Span vs CopyTo in Levenshtein Distance]
**Learning:** While `Span<T>.CopyTo` is efficient, it still takes O(N) time. Swapping Span descriptors is an O(1) operation and should always be preferred in nested loops like the Levenshtein distance algorithm. Additionally, using `ValueTuple` and manual loops instead of LINQ with anonymous objects in search paths significantly reduces heap allocations (~9x reduction).
**Action:** Use O(1) Span swaps in Levenshtein and prefer ValueTuple over anonymous types for performance-critical data processing.
