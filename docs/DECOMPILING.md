# Decompiling DE scripts

Use the fidelity lane when the result will be edited and returned to the game:

```powershell
.\renovice.ps1 decompile C:\private-corpus\Lotus_Script.lua_B .\work\Lotus_Script.fidelity.luau
```

This calls `decompile-mod`. The output uses canonical generated identifiers where original names were stripped or cannot be proven. That is expected. Do not replace an unresolved hash or register name with a plausible gameplay name unless evidence is added to the readability/API layer.

Useful inspection commands:

```powershell
# Decode one prototype as instructions
.\bin\derecomp.exe de-disasm C:\private-corpus\Lotus_Script.lua_B 0

# Verify semantic ownership without rendering
.\bin\derecomp.exe plan-verify C:\private-corpus\Lotus_Script.lua_B

# Record exact child/constant closure indices and captures
.\renovice.ps1 closure-map C:\private-corpus\Lotus_Script.lua_B .\work\Lotus_Script.closures.tsv

# Verify parser/writer byte identity for the unmodified DE container
.\bin\derecomp.exe de-roundtrip C:\private-corpus\Lotus_Script.lua_B
```

`NEWCLOSURE.Bx` indexes the current prototype's child list. `DUPCLOSURE.Bx` indexes a tag-6 closure constant. A flat prototype number alone does not prove closure ownership or the runtime upvalue it captures.

The external corpus is never required for one-file operation and is never copied into this repository. Set `RENOVICE_CORPUS` only for batch certification.
