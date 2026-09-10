# RENOVICE Semantic SDK generator

This deterministic .NET tool joins the standalone repository's API contracts,
corpus-observed catalog, evidence records, negative findings, and native-build
registry into a compact Semantic SDK for the readable renderer.

From the repository root:

```powershell
.\semantic-sdk\build.ps1
.\semantic-sdk\Build\renovice-semantic.exe selftest --workspace .
.\semantic-sdk\Build\renovice-semantic.exe build --workspace .
.\semantic-sdk\Build\renovice-semantic.exe validate --workspace .
.\semantic-sdk\Build\renovice-semantic.exe query RadialDamage --workspace .
```

Published output goes to `knowledge/semantic-sdk` through `WORKSPACE.json`.
Publication is staged and validated before the three coordinated files replace
the prior version. Missing inputs, duplicate identities, malformed tables,
hash drift, conflicting facts, and invalid output fail the operation.

The generated SDK keeps evidence and confidence attached to every fact. Native
facts remain keyed to the exact native build. An observed call shape is not
silently promoted into parameter meaning, side effects, or a dynamic type.
