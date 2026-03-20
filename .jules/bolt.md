## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2026-03-20 - [Advanced Fuzzy Search Optimization]
**Learning:** Even with O(min(N,M)) space, Levenshtein distance remains O(N*M) in time, which can freeze the UI for large clipboard items (>10k chars) when searching. LINQ anonymous objects and `Select` chains in hot search loops create significant GC pressure.
**Action:** Implement a character threshold (e.g., 1000) to skip heavy fuzzy matching for large items. Use `ReadOnlySpan<char>`, `stackalloc` for small buffers (<= 256), and replace LINQ with `ValueTuple`-based loops to minimize heap allocations.
