# Native HUD empty-list contract correction, V86

Positive: exact executable SHA256
CCA46D604A498CD95F0D28E3E8F3EEE8833F5D362666A8E5C820C535F7C2AF93,
registered name hash0x156a31e6, getter RVAs0x1b17710/0x1b9a890. Both zero-count
branches call nil-writing helper RVA0x55bdd0 (tag0); nonempty branches create
array tables through RVA0xfadda0 (tag7), populate copied notification userdata
and return one result. Correct Avatar/HudStatus GetBuffNotifications return
contracts from table to tableOrNil. The stock InventoryControl.GetHudStatus
accessor is verified by HudRedux prototype147 and queue use in76.

Negative: nil is not evidence of a failed getter. V85 discarded error/type,
so its old unavailable reads cannot all be asserted nil. Neither HUD queue nor
Avatar notification list decomposes every engine contributor. Mod effects are
included in final engine upgraded stats, not separately enumerated or inferred.
Current user confirms Efficiency0.45 is correct for their mod loadout.

Validated deterministic generation: symbols258, deep64, catalog225,
high-confidence175, evidence40, negatives26, explicit guessed native links0.
SDK SHA2565A617DC3FC2A15B6E0A24AC935D19CC7FB393CB57539386203CDFB3C8FC65E1C;
symbols SHA2567293D7359EA7A46747B4EEA0A1E06C1E5A94418ECDDABC625E6D2E2B113A1859.
SDK selftests/build/validate PASS. Evidence ID
WF-NATIVE-HUD-EMPTY-LIST-2026-09-15. Runtime capture live acceptance remains
separate from contract/build validation.

Current explanation: central Documentation article
Universal-Buff-Capture-Repair-2026-09-15.md. Reproducible native evidence and
negative tests: runtime RESEARCH/UNIVERSAL_BUFF_CAPTURE_REPAIR_V86_2026-09-15.
