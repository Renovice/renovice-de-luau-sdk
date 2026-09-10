# Current full-corpus revalidation — 2026-09-10

## Hypothesis 1

The historical 5,386/5,386 base source decompile/recompile result was a real
whole-corpus result rather than a 360-file result later described too broadly.

**Result: TRUE.** The retained 2026-07-26 finding explicitly records 5,386
files, zero failures, and states that every shipped script decompiled and
recompiled after the long-path, multi-arm `else`, and lexical-loop fixes. The
older source recovery also retained a separate 5,386/5,386 byte-identical
no-op result for its instruction/container editor. These claims concern
different lanes and must remain separate.

## Hypothesis 2

The current standalone `derecomp.exe` still reaches a strict source and
bytecode fixed point over those 5,386 external stock inputs.

**Result: FALSE.** The raw `decompile-mod` two-cycle audit produced:

| Result | Files |
|---|---:|
| source and rebuilt bytecode fixed after cycle 1 | 5,070 |
| both cycles completed, but source and/or rebuilt bytecode drifted | 315 |
| source-rendering execution error | 1 |
| total | 5,386 |

Across the completed comparisons, prototype-count delta is zero. Aggregate
cycle-1-to-cycle-2 byte delta is 21,946 bytes and aggregate maximum-stack sum
delta is 109. Five deterministic witnesses subsequently remained stable
through cycle 10. No input changed during the audit, and none of the 5,386
first rebuilt containers was byte identical to its stock input.

The current hashes are:

- `derecomp.exe`: `60157E2FD66E884E089AA7762AA7C5CD79C67748A502B7885FF6FEAE5AB46D6C`
- `luau-compile.exe`: `6156A05D9CAF9FA84E58BAECA79A244F47D86B8982F5425F2CA3E23B976ED2A4`
- mode: raw `decompile-mod`

The compact machine-readable partition is in `summary.json`. The 23 MB raw
working report remains excluded because it embeds the private external corpus
path and all per-cycle source/bytecode records.

## Hypothesis 3

The single error came from the standalone packaging operation or an invalid DE
container.

**Result: FALSE.** Both the standalone binary and the current binary in the
source toolchain have the same SHA-256 and reproduce the error. For
`Lotus_Powersuits_Jade_Abilities_Chaos.lua_B`:

- input size: 16,270 bytes;
- input SHA-256:
  `5C0D1B708CA6A365EE5E1E5E73EA03633EF00D696C49B332EFBE13E95C579C14`;
- `de-roundtrip`: 20/20 prototype constant/body re-encodes exact;
- `decompile-mod-raw`: uncaught `std::out_of_range`, `map::at`;
- `decompile-mod-stable`: same exception;
- `decompile-mod`: same exception.

This isolates the current failure to module-source rendering. The file's
retained last-write timestamp predates the historical July 26 clean run, which
supports an emitter regression as the leading explanation. Because the old
full-corpus report did not retain this input's hash and old binary hash, the
precise historical change point remains unproven.

## Conclusion

The old broad-coverage statement was correct for the historical build. The
current toolchain still processes 5,385 of 5,386 inputs through two source
compile cycles, but only 5,070 satisfy the newer strict fixed-point gate. The
one exception and 315 drift cases must be fixed and the same 5,386-file audit
rerun before describing the current source pipeline as full-corpus complete.
