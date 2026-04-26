## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-15 - [FuzzySearch Allocation and Performance]
**Learning:** LINQ operations (`Select`, `Where`, `OrderByDescending`) and anonymous types in search loops cause significant heap allocations (~117KB per 500 items). Using `stackalloc` for small arrays and a manual loop with `ValueTuple` and `List.Sort` (incorporating original index for stability) can drastically reduce allocations and improve performance.
**Action:** Replace LINQ with manual loops and `ValueTuple` for critical search paths; use `stackalloc` for temporary buffers in algorithms like Levenshtein distance.

## 2025-05-15 - [FuzzySearch sequence matching optimization]
**Learning:** The original  implementation converted every character of the haystack to lowercase inside the loop, causing (N)$  calls. For large haystacks (e.g., 3MB clipboard content), this becomes a major bottleneck. By using  for each needle character, we can leverage SIMD-accelerated search and reduce case-insensitive conversion overhead to (M)$ where $ is the needle length. Additionally, an early return as soon as the needle is satisfied prevents unnecessary scanning of the rest of the haystack.
**Action:** Use vectorized  with both case variants for case-insensitive sequence matching; always implement early exits in search loops when the condition is met.

## 2025-05-15 - [FuzzySearch sequence matching optimization]
**Learning:** The original `ContainsInOrder` implementation converted every character of the haystack to lowercase inside the loop, causing O(N) `char.ToLowerInvariant` calls. For large haystacks (e.g., 3MB clipboard content), this becomes a major bottleneck. By using `ReadOnlySpan<char>.IndexOfAny(lower, upper)` for each needle character, we can leverage SIMD-accelerated search and reduce case-insensitive conversion overhead to O(M) where M is the needle length. Additionally, an early return as soon as the needle is satisfied prevents unnecessary scanning of the rest of the haystack.
**Action:** Use vectorized `IndexOfAny` with both case variants for case-insensitive sequence matching; always implement early exits in search loops when the condition is met.
