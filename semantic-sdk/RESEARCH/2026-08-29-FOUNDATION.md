# Semantic SDK foundation closeout — 2026-08-29

## Scope

Build one deterministic, evidence-preserving semantic graph that the DE Luau
decompiler, Ability Editor, and Native Explorer can consume. This work does not
claim that Lua API names identify native C++ functions, and it does not alter
the fidelity renderer or compiler semantics.

## Hypotheses and results

### H1 — Existing knowledge can be merged without erasing authority boundaries

**Result: TRUE.** Schema v1 contains 167 Lua symbols, including 37 deep
contracts and the curated 150-symbol corpus catalog; 20 evidence records, 13
negative findings, two native functions, and four native types remain separate
records with their original confidence/status. The exact native build identity
is embedded in the document.

### H2 — Selected catalog rows equal the complete corpus API census

**Result: FALSE.** Selected symbols account for 34,775 call sites. The complete
census contains 55,709 rows. Both measurements are stored independently so a
consumer cannot mistake curated coverage for total coverage.

### H3 — Lua-to-native links can be inferred safely from similar names

**Result: FALSE.** No such link is proven by the current registries. Generated
SDK output contains zero native links. Links may only be added to
`registry/native_links.tsv` with evidence IDs and must pass exact-build and
reference validation.

### H4 — SDK types can improve readability without changing executable output

**Result: TRUE.** The SDK and legacy evidence paths produced identical
RunnerRush fidelity and readable source. The SDK additionally emitted receiver,
argument, result, and field types. BardMusic produced 16 confirmed field-flow
facts, including numeric radius/falloff and boolean cover/static-contact fields.
Conflicting candidate facts were diagnosed and left canonical.

### H5 — The SDK-enabled readable renderer compiles across the primary ability corpus

**Result: TRUE.** 286/286 fidelity modules and 286/286 readable modules compiled,
covering 6,035 prototypes. Results: 150,521 aliases, 67,836 typed value webs,
zero render failures, and zero compile failures.

### H6 — The underlying decompiler is fully idempotent after this integration

**Result: FALSE, pre-existing and unrelated to the optional SDK path.** The
strict gate reached a byte/source/structure fixed point for 10/90 two-cycle
specimens and 0/5 default ten-cycle specimens. A prior production baseline also
reported 10/90, so this is not an SDK regression. The current aggregate
cycle-one-to-two byte delta is +27,318 bytes. Preserve
`work/logs/semantic-sdk/2026-08-29/idempotence.json` while investigating this
separate M6 emission defect. The full SDK corpus result is preserved beside it
as `readable-corpus.json`.

## Passed gates

- Semantic SDK self-test: 7/7, including compact-feed schema/generator identity.
- Semantic SDK validation: 167 symbols, 20 evidence records, 13 negative
  findings, zero guessed native links.
- DE Luau release gates: all accepted; 0 dropped paths, 0 access loss, 150/150
  behavioral round-trip, 150/150 renderer behavior, and 11/11 API trace.
- Warframe API checker/catalog: PASS; 150 catalog symbols, 100 high-confidence,
  55,709 census rows.
- Native registry: 7/7 self-tests; validator errors 0, warnings 0.

## Safety contract

1. Source and output hashes are mandatory and revalidated.
2. Schema and generator identities must match exactly.
3. Evidence, native-function, native-type, and negative-finding references must
   resolve.
4. Native records must match the exact registered executable build.
5. Missing or conflicting semantic facts remain unknown; consumers must not
   guess.
6. SDK use is optional for fidelity output but fail-closed when explicitly
   requested.
