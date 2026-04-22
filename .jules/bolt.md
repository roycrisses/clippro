## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-15 - [FuzzySearch Allocation and Performance]
**Learning:** LINQ operations (`Select`, `Where`, `OrderByDescending`) and anonymous types in search loops cause significant heap allocations (~117KB per 500 items). Using `stackalloc` for small arrays and a manual loop with `ValueTuple` and `List.Sort` (incorporating original index for stability) can drastically reduce allocations and improve performance.
**Action:** Replace LINQ with manual loops and `ValueTuple` for critical search paths; use `stackalloc` for temporary buffers in algorithms like Levenshtein distance.

## 2025-05-15 - [Safety vs. Optimization in Loops]
**Learning:** Removing bounds checks (e.g., `needleIdx < needle.Length`) in a loop to reduce comparisons can lead to `IndexOutOfRangeException` if the input is empty and the loop assumes at least one element. In `ContainsInOrder`, the query was accessed by index before the first increment.
**Action:** Always include explicit guard clauses (e.g., `string.IsNullOrEmpty`) when simplifying loop conditions for performance to handle edge cases safely.
