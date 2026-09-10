# Repository rules

This repository has two deliberately separate output lanes.

1. The fidelity lane preserves compiler-visible structure and is the only lane used for bytecode fixed-point claims.
2. The readable/API lane adds names, types, callsite records, and contracts as evidence-bound sidecars. It must not silently change executable expressions.

Every semantic fact retains its source, confidence, and limitation. Ambiguous or conflicting facts remain unresolved. Native facts are keyed to the exact native build that produced them. Negative findings are permanent evidence unless new evidence explicitly supersedes them.

Warnings are errors. Generated output must be deterministic and validated before publication. Never describe a compiler fixed point as original-stock byte identity, semantic equivalence, or live gameplay proof. Never add game-script corpora, game installations, injector binaries, bootstrapper sources, or server sources to this repository.
