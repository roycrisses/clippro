## 2025-05-15 - [Levenshtein Distance Optimization]
**Learning:** The original Levenshtein distance implementation used O(N*M) space, which is highly inefficient for large strings (like clipboard content) and can lead to OOM or high GC pressure. Additionally, calling ToLowerInvariant() on large strings before processing creates unnecessary copies.
**Action:** Use O(min(N, M)) space for Levenshtein distance and perform case-insensitive comparisons character-by-character to avoid large string allocations.

## 2025-05-16 - [Search Debouncing and Cancellation]
**Learning:** Performing expensive database queries and fuzzy searches on every keystroke in a search field leads to high CPU usage and UI lag. In this codebase, the `MainViewModel` is duplicated across projects (`Core` and `WPF`), and both implementations lacked any debouncing or cancellation logic.
**Action:** Always implement debouncing (e.g., 300ms) and use `CancellationToken` to cancel previous search tasks when new input is received. Ensure consistency across duplicated ViewModels.
