# Full C# IntelliSense In First Slice

Prodigee Tuts Point will include live C# IntelliSense in the first vertical slice rather than deferring it behind compiler-only feedback. This raises initial implementation cost, but it protects the intended learning experience: exercises should feel like serious coding work with semantic completions, diagnostics/linting/squiggles, formatting, hover/help, and language-aware feedback inside Monaco.

The target quality bar is the Microsoft C# VS Code extension editing experience, except code navigation can be deferred. Completion lists must not be generic Monaco noise: locals and parameters must be recognized from the current buffer, member access should not show keyword/snippet pollution, and semantic item tags should map to meaningful editor icons. The full implementation should add Roslyn code actions/refactorings through a proper language-service bridge rather than fake client-side quick fixes.
