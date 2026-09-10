namespace Renovice.SemanticSdk.Core;

public static class SemanticSdkSelfTest
{
    public static IReadOnlyList<string> Run(Workspace workspace, string toolchainRoot)
    {
        var passed = new List<string>();
        SemanticSdkDocument first = SemanticSdkGenerator.Generate(workspace, toolchainRoot);
        SemanticSdkDocument second = SemanticSdkGenerator.Generate(workspace, toolchainRoot);
        if (!string.Equals(SemanticSdkGenerator.SerializeDocument(first),
                SemanticSdkGenerator.SerializeDocument(second), StringComparison.Ordinal))
            throw new InvalidDataException("Repeated generation is not byte-deterministic.");
        passed.Add("deterministic generation");
        string symbols = SemanticSdkGenerator.SerializeSymbols(first.Symbols);
        string[] symbolLines = symbols.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (!symbolLines[0].StartsWith("schema_version\tgenerator\tid\t", StringComparison.Ordinal)
            || symbolLines.Skip(1).Any(line => !line.StartsWith(
                "1\t" + SemanticSdkGenerator.GeneratorIdentity + "\t", StringComparison.Ordinal)))
            throw new InvalidDataException("Compact symbol feed lost its schema/generator identity.");
        passed.Add("compact feed identity preserved");
        string[] compactHeader = symbolLines[0].Split('\t');
        int idColumn = Array.IndexOf(compactHeader, "id");
        int observedArgumentsColumn = Array.IndexOf(compactHeader, "observed_args");
        int observedOpenArgumentsColumn = Array.IndexOf(compactHeader, "observed_open_args");
        if (idColumn < 0 || observedArgumentsColumn < 0 || observedOpenArgumentsColumn < 0)
            throw new InvalidDataException("Compact symbol feed lost exact observed argument columns.");
        string[] upgradeValue = symbolLines.Skip(1).Select(line => line.Split('\t'))
            .Single(row => row[idColumn] == "lua:method:PowerSuit:GetUpgradeModifiedValue");
        if (upgradeValue[observedArgumentsColumn] != "2;4;5"
            || upgradeValue[observedOpenArgumentsColumn] != "false")
            throw new InvalidDataException(
                "Compact symbol feed widened non-contiguous observed arguments.");
        passed.Add("exact observed argument shapes preserved");
        if (first.Statistics.CatalogSymbols != 225 || first.Statistics.HighConfidenceCatalogSymbols != 175)
            throw new InvalidDataException("Catalog 225/175 invariant failed.");
        passed.Add("catalog 225/175 invariant");
        if (first.Statistics.CorpusCensusRows != 55_709)
            throw new InvalidDataException($"Expected 55,709 census rows, got {first.Statistics.CorpusCensusRows}.");
        passed.Add("55,709-site census invariant");
        if (!first.NegativeFindings.Any(item => item.Result == "FALSE"))
            throw new InvalidDataException("Negative findings were not preserved.");
        passed.Add("negative findings preserved");
        if (!first.NativeTypes.Any(item => item.Status.StartsWith("REJECTED", StringComparison.Ordinal)))
            throw new InvalidDataException("Rejected native types were not preserved.");
        passed.Add("rejected native types preserved");
        if (first.Symbols.Any(item => item.NativeFunctionIds.Count != 0 || item.NativeTypeIds.Count != 0))
            throw new InvalidDataException("Unproven native links were generated without explicit registry rows.");
        passed.Add("no guessed native links");
        return passed;
    }
}
