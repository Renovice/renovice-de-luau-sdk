# Current knowledge synchronization — 2026-09-17

## Hypothesis

The September 10 standalone SDK already contained the current translator and
all later API/runtime semantics.

**Result: FALSE.** Its own verification passed, but comparison with the active
workspace found one changed translator source, five changed API files, four
changed name-data files, three changed supporting tool files, and a newer
Semantic SDK. The standalone graph had 242 symbols; the active graph had 258.

## Synchronized inputs

- current `src/transcode.h`, including the shared string/name constant repair;
- current Warframe API contracts, evidence, negative contracts, catalog, and
  API documentation;
- current name map and verified/merged name databases;
- current ability-behavior and API-catalog build helpers;
- current Semantic SDK source and research records;
- Ice Wave borrowed-value/packet lifetime findings;
- the Viral ID 11 machine-readable live verification;
- the shared-string regression source and 300/150 gate evidence.

The extracted 5,386-file corpus, bootstrapper source, server source, game
installation, runtime logs, and generated scratch output remain excluded.

## Verification boundary

The warnings-as-errors rebuild produced 258 symbols, 64 deep contracts, 225
catalog symbols, 41 evidence records, and 29 negative findings. Compiler,
closure-index, Semantic IR, lowering, readable naming, API positive/negative,
Semantic SDK, ability-behavior, container, wrapper, readable/fidelity compile,
fixture-boundary, and documentation-link gates pass after synchronization.

The selected 360 and full 5,386 external-corpus results remain the separately
dated certificates already under `docs/certificates`. This synchronization did
not rerun or relabel those measurements.
