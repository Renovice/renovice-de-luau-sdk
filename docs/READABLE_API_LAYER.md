# Readable and API translation layer

The readable layer sits above the fidelity translation. It does not replace the compiler and it does not require the bootstrapper or server.

```powershell
.\renovice.ps1 readable C:\private-corpus\Lotus_Script.lua_B .\work\Lotus_Script
```

This produces four coordinated files:

| File | Meaning |
|---|---|
| `Lotus_Script.fidelity.luau` | Verified canonical source used for exact translation work. |
| `Lotus_Script.readable.luau` | Compilable source with evidence-backed identifier aliases. |
| `Lotus_Script.names.tsv` | Every canonical/readable name, confidence, type, and evidence source. |
| `Lotus_Script.calls.tsv` | Every call keyed by prototype, instruction, and source occurrence, including receiver, argument/result webs, order, join basis, contract, and both source spans. |

The renderer fails before publishing if the requested SDK is missing or invalid, readable source does not compile, identifier ownership is ambiguous, or the naming plan changes the renderer's frame/dispatcher strategy.

## API evidence levels

| Level | What may be claimed |
|---|---|
| `LIVE_CONFIRMED` | The recorded signature or behavior was observed in the named private-server/game experiment. |
| `STOCK_BYTECODE` | The call identity/shape or value relationship exists in shipped bytecode or inspected native evidence. |
| `OFFLINE_FIXTURE` | Translator behavior is covered; it is not a real engine contract. |
| `UNRESOLVED` | The fact is explicitly unknown and remains canonical. |

A corpus-stable argument count proves a call shape. It does not by itself prove parameter meaning, receiver class, side effects, authority, lifetime, replication, or a dynamic return type. Detailed rows in `api/warframe/contracts.tsv` carry those claims only when their evidence supports them. Disproved assumptions remain in `negative_contracts.tsv`.

## Checking an authored add-on

```powershell
.\renovice.ps1 api-check .\addon.luau --strict-unknown

# A replacement may subtract untouched stock calls from strict checking.
.\renovice.ps1 api-check .\replacement.luau --baseline .\verified-stock.luau --strict-unknown
```

The checker validates registered names, visible argument counts, and callback arity. It cannot infer every receiver type from plain authored source. The instruction-addressed call map is the stronger artifact for decompiled modules because it can join exact value-web evidence.

## Updating the catalog

`build.ps1` compiles `wf_api_catalog`, regenerates `selected_catalog.tsv` from the bundled 55,709-row census, rebuilds the Semantic SDK, and validates the result. Add evidence before adding a semantic name. Add a negative row when an experiment disproves a contract. Keep native claims keyed to the exact native build in `knowledge/native-analysis/registry/registry.json`.
