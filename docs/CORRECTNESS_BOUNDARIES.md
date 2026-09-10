# Correctness boundaries

Every report must identify the exact binary, Luau frontend, mode, inputs, denominator, and result partition. Use these claims separately:

| Claim | Required evidence | What it does not prove |
|---|---|---|
| Parser/writer container identity | `de-roundtrip` reports exact equality for the input | Decompiled source correctness |
| Raw first-pass parity | Explicit comparison of the first recovered source/bytecode against a stated oracle | Stability in later cycles or gameplay |
| Compiler-closed fixed point | `S1 == S2` and `B1 == B2` byte for byte under the same compiler and mode | `B1 == original stock`, semantic equivalence, or live behavior |
| Original-stock byte identity | Direct byte comparison of the rebuilt container with the original input | Behavior for code paths not exercised |
| Semantic readability | Ownership/IR verification, compilable fidelity/readable twins, complete provenance maps, and zero rejected modules in the stated denominator | Correct guesses for stripped names or every runtime side effect |
| Offline behavior | Source-grounded fixtures or mocked-engine traces with non-vacuous assertions | Native engine behavior, replication, or timing |
| Live gameplay | Exact deployed hashes, game/runtime build, logs, and observed outcome | Other scripts, builds, hosts, or paths |

## Retained measured state

The fresh standalone certificate at
`docs/certificates/standalone-2026-09-10/RESULTS.md` passed 360/360 raw fixed
points, 5/5 default ten-cycle witnesses, and every canonical 300/150 release
gate with zero integrity errors. It independently reports 0/360 original-stock
byte identity. Its current `derecomp.exe` hash is
`60157E2FD66E884E089AA7762AA7C5CD79C67748A502B7885FF6FEAE5AB46D6C`.

Historical evidence remains useful for the defects and broader witness set:

The historical raw closeout in `docs/certificates/raw360/RESULTS.md` records:

- denominator: deterministic 360-file stock sample;
- mode: raw `decompile-mod`;
- fixed point: 360 pass, 0 fail for source and rebuilt bytecode across the measured cycles;
- long witnesses: 11 distinct files through cycle 10 in the final closeout;
- canonical release gates: 300 structural and 150 behavioral inputs passed;
- independent named-access loss: zero across the sample;
- original-stock byte identity: 0/360 for the first rebuilt containers;
- full local corpus boundary: an unfiltered 5,386-file Semantic IR experiment later retained 20 ownership-manifest failures and 8 Semantic IR failures.

Those values remain historical until `tools/verify-corpus.ps1` produces a fresh report for the current repository artifact. Do not describe 360 as the number of scripts the tool can process or as the full game corpus.

## Fail-closed rules

- Missing, duplicate, malformed, or changed corpus inputs fail a certification run.
- Unknown names and conflicting types remain unresolved.
- A renderer that cannot prove complete ownership rejects the module.
- Equal file sizes never count as byte equality.
- A nonzero diagnostic tool exit that means “difference found” must be interpreted by that tool's documented contract; execution failures remain failures.
- No baseline moves merely to make a gate green. Record the before/after evidence and reason.
