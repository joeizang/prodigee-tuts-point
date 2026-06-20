# SwiftPM And SourceKit-LSP For Swift Exercises

Swift exercises will use generated Swift Package Manager workspaces rather than single-file snippets. Each exercise workspace must contain a real `Package.swift`, a library target under `Sources/Exercise`, visible tests, hidden tests, and package settings compatible with SourceKit-LSP.

Swift editor intelligence must use SourceKit-LSP or a SourceKit-backed bridge. Monaco may provide syntax highlighting before the bridge lands, but semantic completion, diagnostics, hover, formatting, and code actions are not considered complete until they come from the Swift toolchain. The target quality bar matches the C# and TypeScript standard: current-buffer-aware locals and parameters, package-aware symbols, diagnostics with line/column ranges, formatting, and refactoring/code actions where SourceKit-LSP supports them.

Swift execution will use the local `swift` toolchain through the same host-process-first runner model used by .NET and Node: strict timeout, output limit, generated run workspace, hidden-test privacy, and later container-ready isolation. Server-side Swift and Vapor milestones should build on the same SwiftPM package shape so CLI, core library, async, HTTP, and Vapor exercises do not fork the workspace model.
