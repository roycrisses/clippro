## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-16 - [FuzzySearch Allocation & Stability Optimization]
**Learning:** Using LINQ with anonymous objects in a search hot-path (`Search<T>`) causes excessive heap allocations and GC pressure. Furthermore, `Enumerable.OrderByDescending` alone might not guarantee stable results for identical scores if the underlying collection isn't sorted, which can lead to "jumping" UI elements during search.
**Action:** Use a manual loop with `List<(T Item, double Score, int Index)>` to avoid anonymous objects and explicitly include the original index in the sorting logic (`OrderByDescending(x => x.Score).ThenBy(x => x.Index)`) to ensure a stable, allocation-efficient search.
