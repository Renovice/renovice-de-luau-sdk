# Avatar velocity contract from caster diagnostics

Hypothesis: the logger's GetVelocity call has a selected API contract. False
before this change: strict source verification reported exactly one unknown.

Hypothesis: pinned stock Lua establishes a zero-explicit-argument Avatar vector
getter. True at the inspected Cloaking and GrappleHookPower sites: the result
feeds Length, Normalize, scalar multiplication and ForceVelocity. The contract
is Vector3 at the Lua surface; native representation/retention and rank-aware
sprint-speed formulas remain unverified. No Lua/native pointer link was added.

Evidence identifier WF-STOCK-AVATAR-VELOCITY-2026-09-15 records original stock
hashes and limitations. See the centralized Caster-Stats-2026-09-15.md guide.
The runtime package preserves the prior contracts/evidence TSVs. Generation and
validation pass with 249 symbols, 55 deep contracts, 225 catalog symbols,
175 high-confidence catalog symbols, 36 evidence records and 21 negatives.
The strict logger source check now has 17 deep and 20 catalog call matches,
zero violations and zero unknowns. These are offline/source claims, not live
API or movement-baseline acceptance.
