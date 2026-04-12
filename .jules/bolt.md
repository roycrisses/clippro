## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2026-04-12 - [Fuzzy Search Optimization]
**Learning:** LINQ queries like `.Select(item => new { Item = item, Score = ... })` allocate an anonymous object for every single item in the collection, leading to high GC pressure during search-as-you-type. Additionally, performing Levenshtein distance on strings with drastically different lengths is wasteful if the similarity threshold cannot be met.
**Action:** Replace LINQ with manual loops and `ValueTuple` to reduce allocations. Use a length-ratio heuristic to skip expensive calculations for obviously poor matches. Use `stackalloc` for small arrays to avoid heap allocations.
