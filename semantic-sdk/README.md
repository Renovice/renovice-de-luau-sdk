# RENOVICE Semantic SDK

Knowledge update 2026-09-16: the V90_R2 combat/buff evidence accepts effective
Strength/Range/Duration/Efficiency and actual native buff type
paths, with source-assignment/dispatch separation, packet-copy identities and
non-additive handler observations preserved. HUD counters are not full upgrade
decomposition; direct live NameTag/base/permanent-loadout remain unavailable.
The native ability-card recipe uses existing verified API contracts and one
shared bonus/Strength helper. Runtime implementation and live receipts remain
in the separate bootstrapper repository; this repository retains the API and
lifetime contracts consumed by the compiler and editors.
No symbols, confidence tiers, native links or compiler certificates are promoted
solely by these documentation updates.

One fail-closed, machine-readable view over RENOVICE's existing Warframe
knowledge:

- Lua API contracts and negative contracts;
- the 55,709-site stock-bytecode API census and curated 225-call catalog;
- exact-build native functions, types, evidence, and rejected findings.

The SDK does not promote catalog hints into native truth. Every symbol retains
its original confidence, status, evidence IDs, and source fingerprints.

## Build and generate

```powershell
.\build.ps1
.\Build\renovice-semantic.exe selftest
.\Build\renovice-semantic.exe build
.\Build\renovice-semantic.exe validate
.\Build\renovice-semantic.exe query GetAvatarOwner
```

The validated deterministic outputs are published under
`shared/semantic-sdk/`:

- `semantic-sdk.json`: full graph for editors and analysis tools;
- `symbols.tsv`: compact API/type feed for the C++ decompiler;
- `manifest.json`: source hashes and output hashes.

Current 2026-09-15 generation: 258 total symbols, 64 deep contracts, 225
catalog symbols, and 175 high-confidence catalog symbols over the pinned
55,709-site census. The full graph contains 41 evidence records and 29 negative
findings across the Lua API and native registries, with zero guessed native
links. The status evidence confirms ID 11 as Viral through the user's Torid
test, correlated battle-log ratios and the existing shipped numeric enum index.
See `RESEARCH/2026-09-15-VIRAL-STATUS-ID-11.md` for current validation and hashes;
the prior generation and Ice Wave runtime-value additions remain recorded in
`RESEARCH/2026-09-14-ICE-WAVE-RUNTIME-CONTRACTS.md`.

V84 adds the stock-verified InventoryControl four-argument
GetUpgradeModifiedValue overload and Avatar GetScriptBlackboard table contract.
Efficiency operation 4 is verified at AlchemistDistill's stock callsite; script
blackboards expose script-owned state without proving complete native buff
attribution. The new negative contracts preserve both limits. The portable
V83/V84 query boundary is retained in
`RESEARCH/2026-09-15-CASTER-EFFICIENCY-BLACKBOARD.md`.

The generator resolves all authoritative paths through the workspace's
`WORKSPACE.json`. Use `--workspace <path>` only for isolated tests.

## Consumer contract

The JSON document and compact TSV are schema version 1. The compact feed embeds
its schema and generator identity on every row; the decompiler rejects a
different schema/generator, malformed rows, duplicate identities, and missing
required columns. Its additive `observed_args` and `observed_open_args` columns
preserve the catalog's exact visible argument shapes. `min_args`/`max_args`
remain an envelope for display and deep contracts; catalog consumers must not
turn a non-contiguous observation such as `2|4|5` into `2..5`. Full-graph
consumers validate the manifest's source/output hashes and exact native build
identity.

- `DEEP_CONTRACT` supplies verified receiver, parameter, field-value, return,
  authority, lifetime, callback, and evidence information.
- `CORPUS_CATALOG` supplies observed call shapes and frequency. It is not
  automatically promoted into a deep contract.
- `HIGH_CONFIDENCE` on a catalog-only row proves its stable observed call
  shape. An `UnresolvedReceiver` owner and opaque parameter meanings stay
  unresolved until separate stock, native, or live evidence promotes them.
- Native functions and types remain keyed to the exact registered executable
  build.
- Lua-to-native links are accepted only from `registry/native_links.tsv` and
  must cite evidence. Name similarity never creates a link.
- Negative and rejected findings remain first-class output; consumers must not
  hide them or reinterpret them as successful inference.

## Runtime ownership and transaction boundary

The SDK's type name does not imply source-language value semantics. In
particular, the live Ice Wave trace proved that the `UpgradedValue` returned by
`DamageData:GetBaseAmount()` was a borrowed live view of its packet, not an
immutable snapshot. Consumers must retain the contract's lifetime field and
must not simplify this wrapper into a number unless the source explicitly calls
`GetModifiedValue()` and stores that numeric result.

The published negative findings also forbid these promotions:

- repeated userdata equality or a native address into logical transaction
  identity;
- exact native-call visibility into an assumption that a matching Lua-entry
  hook also fired;
- an ordinary Luau library name into proof that the DE VM exposes that field;
- a 1x calculation into permission to skip an authoritative packet write after
  another target may have mutated the same boundary.

Editors may display these facts as warnings or authoring guidance. They must
not rewrite executable source, add a cache keyed only by userdata identity, or
claim that raw damage equals visible health removed.

The readable decompiler consumes `symbols.tsv` with `--semantic-sdk`. Ability
Editor resolves the same validated file through `WORKSPACE.json`. Native
Explorer reads the full JSON document and refuses a native-build mismatch.

See `RESEARCH/2026-08-29-FOUNDATION.md` for the hypothesis/evidence closeout and
the remaining measured decompiler limitation.

The V82 caster logger also adds the stock-proven Avatar GetVelocity Vector3
contract. Read `RESEARCH/2026-09-15-AVATAR-VELOCITY.md` for the portable source
evidence and unknowns. The complete live capture remains in the separate
runtime evidence archive.

V85 adds separate stock-grounded Avatar/HudStatus GetBuffNotifications contracts, Resource.GetFullName, the BuffResource GetLocalizeTag context, WeakResource and native IsTimerBuff. These preserve queue versus Avatar-list ownership, raw HUD-unit meanings, no guessed stack counts and no complete hidden-upgrade decomposition. See `RESEARCH/2026-09-15-UNIVERSAL-HUD-BUFFS.md`. Native live capture remains a separate acceptance gate.

V86 corrects Avatar/HudStatus GetBuffNotifications to tableOrNil using the exact current executable's native empty-result branches and preserves errors versus valid empty nil. Its InventoryControl.GetHudStatus receiver attribution is rejected by the V87 stock-value-flow review. Mods remain represented in engine aggregate final stats; no complete per-mod/shard contributor list is invented. See RESEARCH/2026-09-15-NATIVE-HUD-EMPTY-LIST.md and the centralized Universal-Buff-Capture-Repair-2026-09-15.md for retained V86 results.

V87 replaces that receiver with the contextual HudMovieInstigator role, recovered
from mMovie:GetInstigator() in original HudRedux prototype 147. This is not a
native class claim. The associated negative findings retain the wrong receiver,
the shared string/name-hash compiler fault and the dropped diagnostic hook
contract at natural addon activation. The repaired SDK passes selftest,
deterministic generation and validation; native Arcane names/timers remain a
pending V87 live gate. Read
`RESEARCH/2026-09-15-SHARED-BUFF-PIPELINE.md` before using the HUD contract;
complete runtime build and deployment evidence remains in the separate
bootstrapper repository.
