## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-16 - [Fuzzy Search Optimization for Large Strings]
**Learning:** For very large strings (e.g., thousands of characters copied to the clipboard), even an optimized Levenshtein distance calculation becomes a bottleneck. Additionally, using LINQ with anonymous objects in search loops creates unnecessary heap allocations and GC pressure.
**Action:** Skip the expensive Levenshtein distance calculation for strings exceeding a certain threshold (e.g., 1000 characters) after faster checks fail. Use explicit `foreach` loops with `ValueTuple` instead of LINQ `Select` to minimize allocations in the hot search path.
