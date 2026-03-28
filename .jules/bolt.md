## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-16 - [Fuzzy Search Performance & Stability]
**Learning:** `FuzzySearch.Search<T>` used LINQ with anonymous types, causing unnecessary heap allocations and overhead. Furthermore, `List<T>.Sort` is unstable, which can break chronological ordering for clippings with identical scores.
**Action:** Use explicit loops with `ValueTuple` and incorporate the original index into the sort logic to maintain stability. Use `stackalloc` for small arrays in `LevenshteinDistance` and set a 1000-character threshold to skip expensive calculations on large strings.
