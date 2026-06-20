# Native Node Runtime

The sixth `logprobe-typescript` milestone connects the framework-neutral HTTP adapter to native Node-shaped runtime concerns. The project now needs to adapt request streams, normalize headers, write responses, compose dependencies, and build request context without letting Node's mutable APIs spread through every function.

This milestone still avoids a framework. That is a deliberate engineering step. A senior engineer should understand the boundary underneath a framework before relying on framework conveniences. Later Fastify wiring will be easier because the contracts are already small and tested.

By the end, the project should have runtime request adaptation, response writing, dependency creation, and a composed server handler that can be tested with fakes instead of sockets.

The practical standard is that runtime glue must be boring under pressure. During an incident, nobody should need to inspect five handlers to learn how headers are normalized or where request ids are created.

## Rubric

**Correctness**: Request adaptation handles missing method/URL defaults, lower-case header keys, array header values, string chunks, and Buffer chunks. Response writing applies status, every header, and the body. Composed handlers create context and call dependencies exactly once for valid requests.

**Design**: Mutable Node runtime objects stay at the edge. Application contracts remain plain readonly objects. Dependency creation is explicit and small.

**Testing**: Visible and hidden tests cover body chunk decoding, header normalization, response writing, dependency use, and successful handler composition.

**Maintainability**: Runtime wiring remains readable enough to replace with a framework later. No function should import or hide broad infrastructure it does not need.

**Complexity**: Prefer direct adapters over a custom server framework. Do not introduce routers, decorators, plugins, or containers until repeated complexity demands them.
