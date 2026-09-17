# Provenance and repository assembly

This standalone repository was assembled on 2026-09-10 from the active RENOVICE workspace without modifying its source repositories. On 2026-09-17 it was synchronized with the current translator, API/name data, Semantic SDK source, generated SDK, and the intervening runtime-value research. `SOURCE_INVENTORY.json` records the current assembly timestamp and hashes; the external-corpus certificates retain their original measurement dates and are not silently relabeled as fresh runs.

| Standalone path | Source role |
|---|---|
| `src`, `data`, `api`, `cert`, `tests`, `tools` | Current DE Luau toolchain source, knowledge, fixtures, and harnesses. |
| `semantic-sdk` | Current Semantic SDK generator, validator, native-link registry, and self-tests. Generated `bin`, `obj`, and `Build` directories are ignored. |
| `knowledge/research/.../result_consumption_sites.tsv` | Audited 55,709-callsite census required to regenerate the selected API catalog. |
| `knowledge/native-analysis/registry/registry.json` | Exact-build native evidence used by the Semantic SDK. |
| `docs/research` | Compact research records that explain accepted and rejected hypotheses. |
| `docs/certificates/raw360` | Curated raw fixed-point/release evidence rather than the multi-gigabyte generated research tree. |

Excluded material:

- all 5,386 extracted game scripts and other proprietary corpus snapshots;
- bootstrapper/runtime and OpenWF server repositories;
- game installations and injected scripts;
- Ability Studio application source;
- approximately 1.66 GB of regenerable raw research output and 124 MB of historical baseline bundles;
- stale .NET `bin/obj` products and machine-specific paths from the source workspace;
- user-specific absolute paths in operational scripts.

The 625 checked-in `.lua_B` files under `cert/` total about 135 KB. They are small compiler-generated regression fixtures paired with test material, not an extracted game corpus. Their purpose is to keep opcode, control-flow, closure, and API lowering failures reproducible in a clean clone.

`SOURCE_INVENTORY.json` pins the bundled executables and critical evidence files. `MANIFEST.sha256` pins the canonical Git-blob bytes of every tracked file except the manifest itself, which cannot hash itself without recursion. Recreate those files after an intentional release change and commit the changed evidence with the source change.
