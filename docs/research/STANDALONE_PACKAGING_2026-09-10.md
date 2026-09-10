# Standalone packaging evidence — 2026-09-10

## Hypothesis 1

The C++ candidate can run every built-in self-test directly from an isolated
temporary build directory.

**Result: FALSE before staging runtime dependencies.** Global lowering, closure
index, and the 140-assertion Semantic IR verifier passed. The Semantic IR
lowering oracle then failed 0/22 because it resolves `luau.exe` beside the
running `derecomp.exe`; the temporary directory lacked that file. Windows
reported that the expected `work/build/luau.exe` command was not recognized.

**Correction:** `build.ps1` now copies the pinned `bin/luau.exe` and
`bin/luau-compile.exe` into the temporary build directory before candidate
self-tests. This preserves the same-directory dependency contract and avoids a
machine-global fallback.

## Hypothesis 1b

The initially selected certification directories contained both compiled
Warframe API fixtures and the Luau source used by the contract checker.

**Result: FALSE.** The corrected staged-runtime build passed native self-tests
and regenerated the 225/175 catalog, then the contract checker failed because
`cert/warframe_api/src/wf_radial_numeric_damage_callback.luau` was absent.
The compiled `de` fixtures alone cannot prove authored-source contract checks.

**Correction:** the eleven small paired `.luau` sources were copied from the
authoritative certification suite. The failed gate remains recorded here.

## Hypothesis 1c

The selected certification subdirectories retained their original relative
layout in the first standalone copy.

**Result: FALSE.** The 360-file idempotence gate passed, then the canonical
release suite stopped before comparison because it requires
`cert/rt/de_native`; the copied tree contained `cert/rt/rt/de_native`.
Equivalent duplicate nesting affected the other selected fixture folders.

**Correction:** every selected fixture directory was recopied by contents into
its canonical location. Assembly-time nested duplicates are excluded by
`.gitignore`; the release suite must pass from the corrected paths.

## Hypothesis 2

The resulting standalone repository can rebuild native tools, regenerate the
API catalog and Semantic SDK, and pass local fixed-point/readability checks
without the parent workspace.

**Result: TRUE after both packaging corrections.** The complete standalone
build and local verification passed: global lowering; closure index 7/7;
Semantic IR 140/140; lowering 22/22; readable naming 13/13; all positive and
negative API checker fixtures; Semantic SDK 8/8 self-tests and structural
validation; ability behavior 6/6 self-tests; exact container round trip; a
compiler-closed source/bytecode smoke fixed point; and recompilation of both
the fidelity and readable views.

The freshly built `derecomp.exe` hash is
`60157E2FD66E884E089AA7762AA7C5CD79C67748A502B7885FF6FEAE5AB46D6C`,
identical to the pre-assembly current binary. The external-corpus wrapper then
passed 360/360 raw fixed points with zero errors, 5/5 default ten-cycle
witnesses, and every canonical 300/150 release gate. Its separately measured
original-stock byte identity remained 0/360. Sanitized machine-readable proof
is retained under `docs/certificates/standalone-2026-09-10`.

## Hypothesis 3

The first positional operand of `renovice.ps1` can be named `$Input` without
affecting normal PowerShell argument binding.

**Result: FALSE.** A direct `renovice.ps1 decompile <source> <destination>`
smoke test reached the wrapper with an empty value because `$input` is a
PowerShell automatic enumerator and variable names are case-insensitive.

**Correction:** the public positional parameters are now `$Source` and
`$Destination`. The command syntax is unchanged. Decompile, recompile,
readable rendering, closure maps, API checks, and SDK queries all use the new
names.

### Option-forwarding follow-up

A second wrapper smoke established that `api-check` itself ran, but
`--strict-unknown` occupied the generic unused destination position and was
not forwarded; the checker explicitly reported `strict_unknown=no`.
The API branch now prepends that position to its forwarded option array, and
the verify branches forward every remaining positional token. The repeated
gravity-contract test passed and explicitly reported `strict_unknown=yes`.

## Hypothesis 4

Pinned text artifact hashes computed from the assembled Windows working tree
will remain valid after Git applies the repository's LF policy.

**Result: FALSE before normalization.** Comparing worktree bytes with staged
Git blobs found CRLF-to-LF changes in the large name maps, generated Semantic
SDK JSON, standalone proof JSON, and source inventory. A clone would therefore
fail the pinned hashes even though the semantic content was unchanged.

**Correction:** the Semantic SDK JSON serializer now normalizes newlines to LF
before hashing and publication. Pinned tables and proof JSON are normalized to
LF before `SOURCE_INVENTORY.json` and `MANIFEST.sha256` are generated. Final
acceptance requires worktree bytes and staged Git blobs to be identical for
every pinned artifact.

## Hypothesis 5

The historical 5,386/5,386 base decompile/recompile result can be repeated as
a strict two-cycle fixed-point result with the current standalone binary.

**Result: FALSE.** A fresh raw `decompile-mod` run over all 5,386 external
inputs produced 5,070 fixed-point passes, 315 successful two-cycle
decompile/recompile cases with source and/or bytecode drift, and one
source-rendering exception. The error is
`Lotus_Powersuits_Jade_Abilities_Chaos.lua_B`, which terminates with an
uncaught `std::out_of_range` / `map::at` in all three module-source modes.

The Jade input still passes exact `de-roundtrip` for all 20 prototype bodies.
The original current toolchain binary and standalone binary have the same
SHA-256, so the failure is not a packaging mutation. The July 26 historical
record remains valid for its earlier artifacts, but the present emitter needs
a full-corpus regression repair before the current build can inherit that
claim. See `docs/certificates/full-corpus-2026-09-10/RESULTS.md`.
