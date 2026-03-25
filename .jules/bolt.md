## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-20 - [Allocation-Efficient Fuzzy Search]
**Learning:** LINQ with anonymous types (e.g., `Select(item => new { item, score })`) creates a new object on the heap for every item in the collection, leading to high GC pressure during searches. Additionally, reference swapping with `stackalloc` memory in C# is unsafe or requires pointers; `Span.CopyTo` is a safer, high-performance alternative for row-swapping in DP algorithms.
**Action:** Replace LINQ with explicit loops and `ValueTuple` for intermediate results. Use a complexity guard (e.g., 1000 chars) to prevent $O(N \times M)$ hangs on large items. Incorporate the original index into the comparison logic for stable sorting when using `List.Sort` or `OrderBy`.
