# Swift CLI Composition

This milestone makes `logprobe-swift` feel like a real command instead of a folder of isolated functions. The command receives a typed request, resolves input through injected readers, streams lines into the counting core, and renders a stable output body. It still avoids `CommandLine`, stdout, stderr, and process exit codes because those belong in a thin executable adapter later.

The important design move is composition. A mature CLI is not one large function that parses, reads, counts, formats, and exits. It is a set of small contracts wired in a predictable order. That order protects the project: parsing errors stay near raw input, file errors stay near input resolution, counting stays value-oriented, and rendering stays at the edge.

The exercise forces table and JSON output to share the same `LevelCount` data. That matters because output formats should not fork the algorithm. If JSON gets one counting path and table gets another, hidden production bugs appear when the two paths drift.

## Rubric

**Correctness**: The command resolves the selected source, counts supported levels, applies the requested limit, and renders the selected output format exactly.

**Design**: The command core accepts a typed `LogprobeCommandRequest` and injected readers. It does not touch process APIs or real files directly.

**Testing**: Visible and hidden tests cover stdin, file input, table output, JSON output, and controlled file failures.

**Maintainability**: Rendering, input resolution, and counting remain separately understandable. The command function should read like a pipeline.

**Complexity**: Input text is converted into an async line stream and consumed once by the counting function. The command does not introduce extra algorithmic complexity beyond composition.
