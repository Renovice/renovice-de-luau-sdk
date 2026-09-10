# Standalone repository certificate — 2026-09-10

## Result

A clean standalone invocation against an external 5,386-file stock corpus
passed both requested release layers.

| Check | Fresh result |
|---|---|
| Mode | `decompile-mod` |
| Deterministic fixed-point denominator | 360 files |
| Cycle-2 source and rebuilt-bytecode fixed points | 360 pass, 0 fail, 0 process errors |
| Default long witnesses | 5 pass, 0 fail through cycle 10 |
| First rebuilt container vs original stock bytes | 0/360 identical; measured separately and not required by this gate |
| Input/binary/frontend integrity changes | 0 |
| Constant-comparison polarity | 8/8 |
| Named-access normalization | 5/5 |
| Semantic IR lowering | 22/22 |
| Source-grounded Semantic IR behavior | 150/150, 0 different, 0 vacuous |
| Warframe API trace | 11/11, 0 different, 0 vacuous |
| Native NAMECALL preservation | 11 original, 11 rebuilt, exact order |
| Dropped paths / dead tails / access loss | 0 / 0 / 0 |
| Behavioral round trip | 150/150, 0 different, 0 timeout |
| Canonical 300/150 release verdict | ALL GATES PASS |

Pinned executables:

- `derecomp.exe`: `60157E2FD66E884E089AA7762AA7C5CD79C67748A502B7885FF6FEAE5AB46D6C`
- `luau-compile.exe`: `6156A05D9CAF9FA84E58BAECA79A244F47D86B8982F5425F2CA3E23B976ED2A4`

The machine-readable reports are `idempotence.json`, `release-gates.json`, and
`run.json` beside this file. Absolute local paths were replaced with
`<repo>` and `<external-corpus>` before publication; test results, artifact
hashes, per-file hashes, metrics, and failure arrays were retained.

## Boundary

This proves a compiler-closed fixed point for the selected 360 inputs and the
existing release oracles. It does not prove original-stock byte identity,
exhaustive semantic equivalence for all 5,386 scripts, or live in-game behavior.
A separate historical unfiltered Semantic IR scan retains 20 ownership-manifest
and 8 Semantic IR failures over the full local corpus.
