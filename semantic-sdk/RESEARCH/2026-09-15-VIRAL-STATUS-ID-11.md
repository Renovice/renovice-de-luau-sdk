# Viral numeric value contract and existing enum index — 2026-09-15

Hypothesis: status identifier 11's Viral name remains inferred. Result: false
after the user's Torid confirmation and inspection of both existing numeric
enum indexes. The deployed V79 log supplies ten exact stack/amplification
matches. Both active and standalone `enum_index.json` copies map DamageType
11 to `DT_VIRAL`, and 4 to `DT_FREEZE`, with identical SHA-256
`9A2D9B2D3325CFD906C8418974358294BDAFB1AAE037181B2EE400BB1E9AE730`.

The authoritative API registry's existing `RadialDamageData:SetDamagePct`
contract now retains this value and cites
`WF-LIVE-VIRAL-STATUS-ID-11-2026-09-15`. Its original stock signature and
confidence are preserved; the new evidence scopes the live value identity.
This requires generation/validation of the Semantic SDK data, not changing its
method schema, adding a fictitious callable constant or modifying bytecode.

The portable mapping remains in `data/enum_index.json`, the API contract remains
in `api/warframe/contracts.tsv`, and the machine-readable correlated live check
is retained below. Full battle-log captures remain in the separate runtime
evidence archive.
The complete existing index contains 26 DamageType entries. Other entries retain
shipped-index provenance and are not described as individually live-tested
status counters or ordinary stackable procs.

Hypothesis: a missing Brightbonnet event proves no damage occurred. Result:
false as an inference. The user observed approximately 500..800 damage, and
stock `NokkoPowerShroom.lua` has a `RadialDamage` area dispatch; the current
observer covers scripted per-target `DamageDD`. Keep native area processing
coverage separate. No runtime, addon or compiler implementation is changed.

Generation/validation passed with 248 symbols, 54 deep contracts, 225 catalog
entries, 175 high-confidence catalog entries, 35 evidence records, 21 negatives
and zero invented native links. Self-test 8/8 passed. The second publication
is byte-identical for all three artifacts:

| Output | SHA-256 |
| --- | --- |
| `semantic-sdk.json` | `B19AB9931DADDD1EB9953449AC92B0786D0FE47ACF6CACD4CEFD3821ADD5B11D` |
| `symbols.tsv` | `D87CFE56FBEA8B305694EA4804F5013A541D8376DBB9ED29EBAF83223F7A729F` |
| `manifest.json` | `6C594A28B2CEBE7E2218462154930ECB3AEECABDED81CBEFE887011E7C4C79EF` |

`query SetDamagePct` returns the new value note and evidence ID. Full JSON
preserves the notes; the compact TSV preserves contract/evidence identity and
retains its existing schema. No consumer constant-substitution behavior is
claimed from this metadata publication.

[Saved verification](../../docs/research/evidence/viral-sdk-verification.json).
The SDK README was updated to this generation; its original is preserved in
the central documentation's Legacy-Entry-Points snapshots.
