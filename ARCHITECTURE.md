# Architecture

```text
Luau source
  -> official Luau frontend (bin/luau-compile.exe)
  -> Luau bytecode parser (src/luau_bc.h)
  -> DE opcode/container transcoder (src/transcode.h, src/de_container.h)
  -> DE 09 03 .lua_B

DE 09 03 .lua_B
  -> DE container/parser + CFG/value analysis
  -> fidelity renderer (recompilable canonical source)
  -> verified Semantic IR
       -> fidelity twin
       -> readable source
       -> name/provenance map
       -> instruction-addressed API call map
```

The raw translator owns bytecode correctness. The readability layer consumes verified structure and may rename identifiers only when provenance permits it. It emits coordinated artifacts and fails before publication if the readable source does not compile, ownership is ambiguous, or a requested Semantic SDK is invalid.

The API layer is data driven:

- `api/warframe/contracts.tsv` stores detailed signatures, authority/lifetime statements, confidence, and evidence IDs.
- `api/warframe/selected_catalog.tsv` stores corpus-observed call shapes generated from the pinned callsite census.
- `api/warframe/negative_contracts.tsv` preserves disproven assumptions.
- `knowledge/native-analysis/registry/registry.json` stores native-build-keyed evidence.
- `semantic-sdk/` validates and joins those sources into `knowledge/semantic-sdk/`.

The compiler does not depend on the injector or server. Deployment remains a separate consumer operation, which prevents runtime code and server behavior from becoming hidden compiler prerequisites.
