## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-20 - [Fuzzy Search Allocation and Performance Optimization]
**Learning:** LINQ operations like `Select`, `Where`, and `OrderBy` with anonymous objects create significant heap pressure (30KB+ for 500 items). Using `stackalloc` with `Span<int>` in the Levenshtein distance algorithm avoids heap allocations for common string sizes (<256 chars), and O(1) row swapping with `Span` is essential for performance.
**Action:** Replace LINQ with explicit loops and `ValueTuple` for intermediate search results. Use `stackalloc` for small arrays and O(1) row reference swaps for `Span` variables. Add length thresholds to skip expensive algorithms on very large strings.
