# Ice Wave runtime contracts Semantic SDK closeout — 2026-09-14

## Scope

Publish the Ice Wave engine-wrapper and hook-boundary findings without changing
the 225/175 curated API catalog, inventing native links, or presenting runtime
behavior as compiler correctness.

## Hypotheses and results

### H1 — `UpgradedValue` can be represented as an immutable numeric return

**Result: FALSE.** The exact V72 Ice Wave trace shows a retained wrapper change
from the stock reading `1771` to the installed reading `5313` when its owning
packet was mutated. `DamageData:GetBaseAmount` remains typed
`UpgradedValue`, but its lifetime now states that it is a borrowed packet-local
live wrapper at the proven callsite. A consumer may obtain a stable number only
from an explicit numeric read such as `GetModifiedValue()`.

### H2 — Userdata or pointer equality is enough to join combat transactions

**Result: FALSE.** Stock creates `DamageData` inside its target loop while the
live native allocator reused one printed address for consecutive logical
packets. The SDK publishes this as a negative finding. It does not invent an
alternative universal object identifier.

### H3 — Lua-entry and native-call hooks share one coverage guarantee

**Result: FALSE.** The V69 native `DamageDD` hook ran at prototype 7 instruction
64 while `luaCalls[7].before` ran zero times. The negative finding requires
consumers to keep hook-layer evidence separate.

### H4 — Ordinary Luau library membership can be assumed inside DE's VM

**Result: FALSE.** The tested target environment exposed `math.min` but
`math.huge` was nil, causing 150 callback failures. The SDK records the exact
negative and does not claim a complete inventory of DE's `math` table.

### H5 — The new facts require changing the bytecode catalog identities

**Result: FALSE.** The catalog remains exactly 225 `CORE` identities and 175
overlapping `HIGH_CONFIDENCE` identities over 55,709 stock callsites. The
change adds evidence, lifetime detail, and negative contracts.

### H6 — Publishing the wrapper contract lets the readable renderer safely name every related value

**Result: FALSE, with correct fail-closed behavior.** A consumer smoke over the
V74 Ice Wave addon emitted exactly
`READABLE_ALIAS_AMBIGUOUS:proto=27:web=25` and
`READABLE_TYPE_AMBIGUOUS:proto=27:web=25`. The web is both the second element
of the hook payload and the receiver of `GetBaseAmount`, so one friendly source
role and one ownership type are not uniquely proven. It remained canonical as
`frame_27[25]`. The renderer accepted 288 other aliases and 69 other types;
the readable source recompiled to 13,152 bytes and reparsed as 33 prototypes.
This is an expected evidence boundary, not permission to suppress the warning
or guess a name.

## Published generation

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `semantic-sdk.json` | 330,813 | `99EB987583905C7249F8D426AA2714A727741694BC2BEFD7161FFD8945BBDC09` |
| `symbols.tsv` | 52,412 | `683F03751D8EE506FD8122E77E4C9225D84AB885E9BD7C52C61AB3CA123201CA` |
| `manifest.json` | — | `F307E1CF72E5E8C998A07383E027948E03DEF6E347D90469C64CC5EF62A66964` |

The validated graph contains:

- 248 total symbols;
- 54 deep contracts;
- 225 catalog symbols and 37,615 selected-catalog callsites;
- 34 evidence records;
- 21 negative findings;
- two exact-build native functions and four native types;
- zero explicit Lua-to-native links.

## Gates

```text
PASS SDK build with zero warnings and zero errors
PASS self-test 8/8
PASS catalog 225/175 invariant
PASS 55,709-site census invariant
PASS exact observed argument shapes
PASS negative findings preserved
PASS no guessed native links
PASS generation and validation
PASS second generation byte-identical for all three published artifacts
PASS query GetBaseAmount returns the new live-wrapper lifetime and evidence ID
PASS decompiler consumer kept the one ambiguous hook-payload web canonical
PASS consumer-readable source recompiled and reparsed as 33 prototypes
```

## Consumer boundary

The decompiler and editor may show the lifetime warning and evidence links.
They must not convert an engine wrapper into a snapshot, infer transaction
identity from addresses, infer Lua-hook execution from a native stack, or turn
visible pool loss into DE's complete native damage formula. V73 gameplay was a
user-reported live pass; V74 battle-log startup and live records remain separate
pending evidence.
