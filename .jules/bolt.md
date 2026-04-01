## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-20 - [Fuzzy Search Memory & CPU Optimization]
**Learning:** LINQ operations like `Select` with anonymous objects and `OrderByDescending` create significant GC pressure in hot paths like real-time search. Using `stackalloc` and `Span<int>` for small strings in `LevenshteinDistance` eliminates heap allocations for most queries. However, aggressive length-difference heuristics can break search expectations (e.g., finding short keywords in long text); a more generous threshold is needed to balance performance and correctness.
**Action:** Replace LINQ with manual `foreach` loops and `ValueTuple` for intermediate search results. Use `stackalloc` for `Span<int>` buffers when length < 256. Keep heuristics generous (e.g., 1000 char difference) to maintain original search behavior while still skipping extreme cases.
