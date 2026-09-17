# Ability API catalog 225 closeout — 2026-09-08

## Scope

Publish the ability-focused API expansion through the Semantic SDK while
preserving the distinction between stable stock call shape, detailed receiver
contracts, native identity, and live gameplay behavior.

## Hypotheses and results

### H1 — The SDK can publish exactly 225 CORE and 175 high-confidence catalog identities

**Result: TRUE.** Generation and validation passed with 225 catalog symbols and
175 overlapping HIGH_CONFIDENCE symbols. All 225 are supported by the pinned
55,709-row stock-bytecode census. The selected rows account for 37,615 corpus
sites and contain 30 detailed contracts.

### H2 — Stable corpus shape is sufficient to invent missing owner and behavior details

**Result: FALSE.** Catalog-only rows retain `UnresolvedReceiver` and unresolved
parameter/return meaning where no separate evidence exists. HIGH_CONFIDENCE on
such a row proves the stable method identity and call shape only.

### H3 — Eight frequently useful controls have exact stock receiver/signature evidence

**Result: TRUE for the stock contract.** `SecondaryScriptArgs:HasArgs`,
`InputControl:EnableJump`, `InputControl:EnableCrouch`,
`MotionControl:SetForceWalk`, `InputControl:SetStopMovement`,
`InventoryControl:SetWeaponsEnabled`, `PowerSuit:SetAbilitiesEnabled`, and
`DamageControl:RemoveAllProcs` were added or promoted with exact receiver,
visible argument count/type, and return shape. No live gameplay, timing,
authority, or replication claim was added.

### H4 — Similar Lua and native names establish a Lua-to-native link

**Result: FALSE.** The generated SDK still contains zero explicit native links.
Only `registry/native_links.tsv` plus resolving evidence may create one.

## Final generation

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| `semantic-sdk.json` | 310,573 | `DE1BAAB66C873BBAE3408E3997B1114151B488A71201D1C2D59ED6516443B37E` |
| `symbols.tsv` | 50,723 | `1E409754D9F5127DCD796F84AF1315EDDFA597F96AB26858A82836FF356A78DD` |
| `manifest.json` | 2,123 | `ED602011A61DFBC8EA04AA53AEF8DBF661758BC65B7A707F5109B8235D7497D4` |

The full validated graph contains 241 symbols, 46 deep contracts, 225 catalog
rows, 37,615 selected-catalog sites, two native functions, four native types,
23 evidence records, 16 negative findings, and zero explicit native links.

## Passed gates

```powershell
.\build.ps1
.\Build\renovice-semantic.exe selftest
.\Build\renovice-semantic.exe build
.\Build\renovice-semantic.exe validate
```

- SDK build: zero warnings and zero errors.
- Self-test: 8/8 passed.
- Build and validation: passed.
- A second publish produced zero output-hash changes.
- API catalog generator/checker and smoke tests passed at the exact 225/175
  gates.

## Consumer boundary

Consumers may use catalog shape to find and validate exact calls. They may use
deep contracts for the receiver/parameter/return facts those rows explicitly
state. They must not treat lexical similarity, an unresolved receiver, a
direct-call list, or a Lua/native name match as runtime semantics.
