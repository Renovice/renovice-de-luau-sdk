# Recompiling to DE bytecode

Compile ordinary Luau source directly into a DE `09 03` container:

```powershell
.\renovice.ps1 recompile .\work\Lotus_Script.fidelity.luau .\work\Lotus_Script.lua_B
.\bin\derecomp.exe de-disasm .\work\Lotus_Script.lua_B 0
```

`derecomp.exe` launches `luau-compile.exe` from the same `bin` directory, parses its bytecode, lowers supported Luau opcodes into DE's mapping, and writes the DE container. Both executables therefore belong to the artifact identity of a result.

For an edit, use this sequence:

1. Preserve the original `.lua_B` outside the repository.
2. Produce fidelity source with `decompile-mod`.
3. Make the smallest intentional source change.
4. Run `api-check` for changed native calls. For a full replacement, pass the verified stock source as a baseline with `--baseline <stock.luau> --strict-unknown`.
5. Recompile to a new path.
6. Re-decompile and recompile that result, then compare the two rebuilt bytecode files. `tools/verify-corpus.ps1` automates this for a corpus sample.
7. Deploy and test the exact hashed artifact through the separate runtime repository.

The compiler intentionally drops or spells some Luau frontend hints differently while preserving the DE operation represented by the toolchain. Consult `OPCODE_MAP.md`, `METHODOLOGY.md`, and the retained negative findings before changing an opcode mapping. Never infer an opcode by its apparent name in an old table.
