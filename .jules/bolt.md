## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2024-05-20 - [FuzzySearch Allocation & Performance Optimization]
**Learning:** Using LINQ with anonymous types in a high-frequency search loop causes massive allocations (one object per item), leading to GC pressure. A length-based fast-exit in Levenshtein similarity can skip expensive calculations for strings that are guaranteed to be below the threshold.
**Action:** Use manual loops with ValueTuples instead of LINQ for search results, and add a length-difference check (e.g., |len1 - len2| > maxLen * 0.7) before running Levenshtein.
