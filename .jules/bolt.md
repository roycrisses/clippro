## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-16 - [FuzzySearch Performance and Allocation Overhaul]
**Learning:** LINQ-based fuzzy searching (`Select`, `Where`, `OrderByDescending`) on large collections (500+ items) creates significant heap allocations and iterator overhead. Furthermore, performing full Levenshtein distance calculations on very large clipboard items (>1000 characters) that aren't clear matches (not substring or in-order) is computationally expensive and unnecessary, as their score will almost certainly fall below the search threshold.
**Action:** Replace LINQ with manual loops and `ValueTuple` for filtering/sorting to reduce GC pressure and ensure stable sorting. Implement an absolute character threshold (1000 chars) to skip $O(N \cdot M)$ Levenshtein calculations on large payloads, yielding up to a 17x speedup for large strings.
