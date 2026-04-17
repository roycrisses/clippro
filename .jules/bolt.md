## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2026-01-20 - [Fuzzy Search Heuristics and Memory Optimization]
**Learning:** Using `stackalloc` for small temporary arrays (like Levenshtein rows < 256 chars) eliminates heap allocations entirely for most searches. Additionally, a simple length-ratio check (`1 - |L1-L2|/max(L1,L2) < threshold`) can skip the expensive O(N*M) Levenshtein calculation for strings that are mathematically impossible to match. Replacing LINQ with manual loops and `ValueTuple` for sorting reduces per-search allocations from ~117KB to <1KB.
**Action:** Always consider mathematical early exits for DP algorithms and use `stackalloc` with `Span<T>` for small, temporary buffers. Avoid LINQ in tight loops where memory pressure is a concern.
