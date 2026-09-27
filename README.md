# RENOVICE DE Luau SDK

A standalone, GitHub-ready toolchain for the `09 03` DE Luau containers used by Warframe. It packages the working decompiler/recompiler, opcode and name knowledge, deterministic certification fixtures, and an optional evidence-backed readability/API layer in one repository.

The repository intentionally excludes the extracted game-script corpus, the runtime bootstrapper, game files, and OpenWF server code. Point corpus audits at your own external extraction with `RENOVICE_CORPUS`.

## Start here

The checked-in Windows executables make the basic workflow immediately usable:

```powershell
# DE bytecode -> recompilable fidelity source
.\renovice.ps1 decompile C:\path\Ability.lua_B .\work\Ability.fidelity.luau

# Source -> DE 09 03 bytecode
.\renovice.ps1 recompile .\work\Ability.fidelity.luau .\work\Ability.rebuilt.lua_B

# Fidelity source plus readable source, provenance, and exact callsite map
.\renovice.ps1 readable C:\path\Ability.lua_B .\work\Ability

# Check intentional API calls before compiling an add-on
.\renovice.ps1 api-check .\my-addon.luau

# Local self-tests and a compiler-closed smoke round trip
.\verify.ps1
```

Run `./build.ps1` to rebuild every native executable with warnings as errors, regenerate the API catalog from its pinned census, build and validate the Semantic SDK, and run the local verification suite. The build discovers `g++.exe` through `-Cxx`, `$env:CXX`, `PATH`, or the standard MSYS2 UCRT64 location.

Before committing an intentional release change, stage it and run `python .\tools\update_manifest.py --write`, then stage the regenerated `MANIFEST.sha256`. The manifest hashes the canonical Git-blob bytes of every other tracked file, so line-ending conversion in a checkout cannot silently change the recorded release identity.

## What each lane means

| Lane | Output | What it establishes |
|---|---|---|
| Container | `de-roundtrip` | Parser plus writer can reproduce the exact input container for the tested file. |
| Fidelity | `decompile-mod` then `recompile` | Recompilable source and a compiler-closed bytecode fixed point for the tested cycles. |
| Readable | `semantic-ir-render-module-readable` | A verified fidelity twin, readable view, name provenance, and instruction-addressed API calls. |
| API contract | `wf_api_check` and Semantic SDK | Known call shapes and evidence-backed contracts; unknowns stay visible. |
| Live game | external injection and gameplay test | Actual runtime behavior for that exact artifact and game build. |

These claims are separate. A fixed point does not mean that rebuilt bytes equal the original stock bytes, and neither claim proves live behavior.

## U44 compatibility

The 2026-09-27 update adds an explicit U44 build profile and reversible opcode/name lowering. See [profiles/u44/README.md](profiles/u44/README.md) for commands and limitations. Existing default commands retain the U43 contract; normalize U44 bytecode before using those decompiler modes. This profile does not automatically rebind changed stock modules, prototypes or callsites.

## Current evidence boundary

The portable repository was synchronized with the active translator, API
contracts, name data, Semantic SDK, and post-Ice-Wave runtime-value findings on
2026-09-17. The rebuilt current SDK contains **258 symbols, 64 deep contracts,
225 catalog symbols, 41 evidence records, and 29 negative findings**. This
source/API synchronization does not replace the separately dated external
corpus certificates below; rerun the applicable corpus gate against your exact
inputs before making a current whole-corpus claim.

The historical base source pipeline did reach **5,386/5,386 files decompiled and recompiled** on 2026-07-26. A separate instruction/container editor also recorded a **5,386/5,386 byte-identical no-op round trip**. Those are valid historical results for their then-current artifacts; the source compiler result was never a claim that its rebuilt containers matched the stock containers byte for byte.

A fresh standalone run on 2026-09-10 records a deterministic 360-file sample passing the raw `decompile-mod` source-and-bytecode fixed point with zero errors, plus the canonical 300/150 release gates. The same run measures the first rebuilt containers as **0/360 byte-identical to their original stock containers**.

The current binary was also revalidated against all 5,386 external stock inputs. The strict two-cycle `decompile-mod` result is **5,070 fixed-point passes, 315 successful decompile/recompile cases with cycle drift, and 1 source-rendering exception**. The exception is `Lotus_Powersuits_Jade_Abilities_Chaos.lua_B`; its container parser/writer round trip remains byte exact, while every current module-source rendering mode terminates at an uncaught `std::out_of_range` / `map::at`. This current result does not invalidate the historical 5,386/5,386 pass, but it means the present source emitter cannot truthfully carry that full-corpus claim until the regression and the 315 stability drifts are closed.

A separate unfiltered 5,386-file Semantic IR experiment reported 20 ownership-manifest failures and 8 Semantic IR failures. That optional readability result is not a base decompile/recompile failure count. See [Correctness boundaries](docs/CORRECTNESS_BOUNDARIES.md), the [current full-corpus revalidation](docs/certificates/full-corpus-2026-09-10/RESULTS.md), the [fresh standalone certificate](docs/certificates/standalone-2026-09-10/RESULTS.md), and the [historical raw closeout](docs/certificates/raw360/RESULTS.md).

Always rerun the relevant gate against the current binaries and your exact corpus before making a current claim. Historical proof files are evidence for their pinned artifacts, not a promise about an arbitrary build.

## Repository map

| Path | Purpose |
|---|---|
| `src/` | C++ DE container parser, Luau parser, transcoder, decompiler, Semantic IR, and renderers. |
| `bin/` | Pinned Windows tools for immediate use. |
| `data/` | Opcode/name maps and recovered metadata required by translation. |
| `api/warframe/` | API contracts, evidence, negative findings, selection seeds, and generated catalog. |
| `semantic-sdk/` | Deterministic C# generator/validator for the compact readable/API sidecar. |
| `knowledge/` | Pinned census/native inputs and generated standalone Semantic SDK. |
| `cert/` | Synthetic fixtures and gate harnesses. The 625 `.lua_B` fixtures total about 135 KB; they are not the game corpus. |
| `docs/research/` | Concise positive and negative research findings. |
| `docs/certificates/` | Curated historical proof records and hashes. |
| `tools/` | API catalog/checker, behavior catalog, and focused research utilities. |

Read [Architecture](ARCHITECTURE.md), [Decompiling](docs/DECOMPILING.md), [Recompiling](docs/RECOMPILING.md), [Readable/API layer](docs/READABLE_API_LAYER.md), and [Provenance](docs/PROVENANCE.md) before changing the translator.

## External corpus certification

The corpus is never committed. Set it explicitly and run the wrapper:

```powershell
$env:RENOVICE_CORPUS = 'D:\private\de-luau-stock'
.\tools\verify-corpus.ps1 -Sample 360 -RunReleaseGates
```

The report states the denominator, mode, binary/frontend hashes, fixed-point partition, and original-byte result separately. Add `-RequireOriginalByteIdentity` only when that stricter property is actually required; it is expected to fail for the retained 360 stock sample.

## Licensing

No license has been selected for the RENOVICE source in this assembled repository. That decision belongs to the project owner before public distribution. The bundled Luau components retain their upstream license in `third-party/luau/LICENSE.txt`; see `THIRD_PARTY_NOTICES.md`.
