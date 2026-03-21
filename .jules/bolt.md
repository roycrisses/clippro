## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2026-01-20 - [Fuzzy Search Memory & CPU Guardrails]
**Learning:** Even with O(N) space, Levenshtein distance remains $O(N \times M)$ in CPU time. For a clipboard manager where items can be megabytes of text, fuzzy search on the full content during every keystroke is a performance trap. Additionally, LINQ pipelines with anonymous objects in the search loop create significant GC pressure (approx. 1 heap allocation per item searched).
**Action:** 1) Implement a 1000-character threshold for fuzzy search; fallback to exact/ordered matching for larger strings. 2) Use `stackalloc` for small string distances (<256 chars) to eliminate heap allocations for typical queries. 3) Replace LINQ anonymous objects with `ValueTuple` and explicit loops in `Search<T>` to reduce heap allocations by ~9x.
