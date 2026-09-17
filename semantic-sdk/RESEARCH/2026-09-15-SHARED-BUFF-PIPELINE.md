# Shared buff pipeline and receiver correction

The V86 session succeeds for damage/aggregate stats but captures no named buffs.
All 525 Avatar results are tables rejected by the broken emitted comparison;
all 525 InventoryControl HUD chains throw. The startup getter hooks disappear
when natural addon activation replaces their method contract.

Hypothesis: InventoryControl is the stock GetHudStatus receiver. **False.**
Original HudRedux prototype 147 obtains mMovie:GetInstigator(), then calls
GetHudStatus on that result. Prototype 76 consumes the returned queue owner.
Use the contextual SDK role HudMovieInstigator, not an invented native class.
The native GetHudStatus record is hash 0xe7fb5ca3, file offset 0x20cb620,
function RVA 0x1a1bfe0 on executable SHA256
CCA46D604A498CD95F0D28E3E8F3EEE8833F5D362666A8E5C820C535F7C2AF93.
Unverified draft instruction PCs were removed before output publication.

Final consistency review found the dependent HudStatus.GetBuffNotifications
note still claiming InventoryControl as its upstream owner. That positive note
is corrected and cites the same stock receiver correction. The first package
is retained under an explicit REJECTED_SDK_NOTE name; only the final package
contains the consistent SDK generation. A regression assertion rejects the
old positive note anywhere in the generated JSON. No additional gameplay
branch or native-class guess is introduced.

Hypothesis: readable fidelity text proves an emitted literal is a string.
**False.** Six value LOADK positions in the V86 observer contain name hashes;
the shared compiler repair preserves literal and hashed contexts independently.
The repaired artifact audits 321 LOADK positions with zero such hashes.

Hypothesis: startup diagnostics survive an addon-only activation method list.
**False.** Both activation paths now merge addon and optional diagnostic methods.
This is generic runtime contract handling, not an ability-specific enabler.

Generation retains 258 symbols, 64 deep contracts, 225 catalog symbols, 41
evidence records, 29 negative findings and zero guessed native links. Eight SDK
selftests and validate pass. The 55,709-site census is not a fresh census run.
Returned tables contain native copied notification userdata; empty native vectors
return nil on the pinned executable. No inferred stack meanings or complete
per-mod/shard decomposition are added. Names/timers remain pending live V87.

The complete runtime source/fingerprint/deployment evidence remains in the
separate bootstrapper repository's
`SHARED_BUFF_PIPELINE_REPAIR_V87_2026-09-15` research case. This file retains
the portable hypothesis verdicts and Semantic SDK boundary.
