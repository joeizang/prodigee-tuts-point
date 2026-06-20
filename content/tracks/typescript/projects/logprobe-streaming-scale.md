# Streaming and Scale

The fourth `logprobe-typescript` milestone changes the project from whole-input thinking to streaming thinking. The point is not to memorize Node stream APIs. The point is to design core logic that can consume lines as they arrive and produce deterministic summaries without keeping the entire input in memory.

This is where the project starts to feel like a serious server-side engineering exercise. A log tool that only works for toy strings is not enough. A useful tool must tolerate large files, slow sources, repeated levels, malformed noise, and stable output requirements.

By the end, the project should consume an `AsyncIterable<string>`, count log levels incrementally, produce bounded top-N summaries, and assemble those pieces into a small streaming runner. The full implementation later can connect this core to Node `readline`, file streams, HTTP request streams, or queue consumers.

## Rubric

**Correctness**: Async iterables are consumed with `for await...of`. Counts are case-normalized where required. Malformed or empty lines do not corrupt valid counts. Top-N output sorts by count descending and key ascending for ties.

**Design**: Streaming consumption is separated from sorting and formatting. The runner composes smaller functions instead of hiding parsing, counting, limiting, and output in one monolith.

**Testing**: Tests use async generators, not only arrays. They cover delayed sources, repeated levels, malformed lines, zero limits, tie ordering, and final stdout formatting.

**Maintainability**: The code remains readable enough to debug during an incident. Each function has one reason to change, and the output format is deterministic for scripts.

**Complexity**: Memory should grow with distinct summary keys, not total input lines. Sorting all distinct keys is acceptable for this milestone; a heap or approximate counter belongs only after profiling or stronger requirements.
