## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-16 - [Fuzzy Search Memory & Algorithmic Optimization]
**Learning:** Even with O(min(N, M)) space, allocating arrays on the heap for every comparison in a long list (e.g., 500 clippings) causes significant GC pressure. Additionally, LINQ projections with anonymous objects (`.Select(item => new { Item = item, Score = ... })`) create unnecessary allocations for every single item before filtering.
**Action:** Use `stackalloc` for small arrays (<256 items) to eliminate heap allocations for common search strings. Replace LINQ projections with manual loops and `ValueTuple` (structs) to avoid anonymous object allocations. Implement early exits for length-difference thresholds to skip expensive calculations entirely.
