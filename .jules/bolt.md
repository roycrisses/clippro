## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-16 - [FuzzySearch Heuristics and Allocations]
**Learning:** Arbitrary length-difference short-circuits (e.g., `Math.Abs(L1-L2) > 1000`) are flawed because similarity is relative; a 1000-char difference is small for very large strings. Additionally, LINQ (`Select`, `Where`, `OrderByDescending`) in hot search loops creates significant garbage (anonymous objects, iterator state machines).
**Action:** Use a 1000-character absolute threshold for the strings themselves to avoid expensive Levenshtein on massive clipboard text. Replace LINQ with manual loops and `ValueTuple` for a stable, allocation-efficient search.
