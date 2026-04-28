## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-15 - [FuzzySearch Allocation and Performance]
**Learning:** LINQ operations (`Select`, `Where`, `OrderByDescending`) and anonymous types in search loops cause significant heap allocations (~117KB per 500 items). Using `stackalloc` for small arrays and a manual loop with `ValueTuple` and `List.Sort` (incorporating original index for stability) can drastically reduce allocations and improve performance.
**Action:** Replace LINQ with manual loops and `ValueTuple` for critical search paths; use `stackalloc` for temporary buffers in algorithms like Levenshtein distance.

## 2025-05-15 - [Binary Artifact Leakage]
**Learning:** Creating temporary benchmark projects or scripts can inadvertently lead to committing large binary artifacts (bin/obj) if not carefully cleaned up. This bloats the repository and violates standard practices.
**Action:** Always manually delete temporary project folders and files (e.g., rm -rf benchmark/) before submitting a PR.
