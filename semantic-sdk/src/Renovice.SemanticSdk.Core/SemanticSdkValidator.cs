using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Renovice.SemanticSdk.Core;

public static partial class SemanticSdkValidator
{
    public static void ValidateDocument(SemanticSdkDocument document)
    {
        var errors = new List<string>();
        if (document.SchemaVersion != 1) errors.Add($"Unsupported schema version {document.SchemaVersion}.");
        if (!string.Equals(document.Generator, SemanticSdkGenerator.GeneratorIdentity, StringComparison.Ordinal))
            errors.Add($"Unexpected generator '{document.Generator}'.");

        RequireUnique(document.Sources.Select(item => item.Id), "source ID", errors);
        RequireUnique(document.Sources.Select(item => item.Path), "source path", errors);
        RequireUnique(document.Symbols.Select(item => item.Id), "symbol ID", errors);
        RequireUnique(document.NativeFunctions.Select(item => item.Id), "native function ID", errors);
        RequireUnique(document.NativeTypes.Select(item => item.Id), "native type ID", errors);
        RequireUnique(document.Evidence.Select(item => item.Id), "evidence ID", errors);
        RequireUnique(document.NegativeFindings.Select(item => item.Id), "finding ID", errors);

        var evidence = document.Evidence.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var nativeFunctions = document.NativeFunctions.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var nativeTypes = document.NativeTypes.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);

        foreach (ApiSymbol symbol in document.Symbols)
        {
            string expectedId = SemanticSdkGenerator.SymbolId(symbol.Kind, symbol.Owner, symbol.Name);
            if (!string.Equals(symbol.Id, expectedId, StringComparison.Ordinal))
                errors.Add($"Symbol ID mismatch: '{symbol.Id}' should be '{expectedId}'.");
            if (string.IsNullOrWhiteSpace(symbol.Kind) || string.IsNullOrWhiteSpace(symbol.Owner)
                || string.IsNullOrWhiteSpace(symbol.Name))
                errors.Add($"Symbol '{symbol.Id}' has an empty identity component.");
            if (symbol.Hash is not null && !NameHash().IsMatch(symbol.Hash))
                errors.Add($"Symbol '{symbol.Id}' has invalid hash '{symbol.Hash}'.");
            if (symbol.MinArguments is < 0 || symbol.MaxArguments is < 0
                || symbol.MinArguments > symbol.MaxArguments)
                errors.Add($"Symbol '{symbol.Id}' has invalid argument bounds.");
            if (symbol.SourceKinds.Count == 0)
                errors.Add($"Symbol '{symbol.Id}' has no source kind.");
            if (string.IsNullOrWhiteSpace(symbol.Confidence) || string.IsNullOrWhiteSpace(symbol.Status))
                errors.Add($"Symbol '{symbol.Id}' has an empty confidence/status boundary.");
            if (symbol.SourceKinds.Contains("DEEP_CONTRACT", StringComparer.Ordinal)
                && symbol.MinArguments is not null && symbol.Parameters.Count != symbol.MinArguments
                && symbol.MinArguments == symbol.MaxArguments)
                errors.Add($"Deep contract '{symbol.Id}' has {symbol.Parameters.Count} parameters but arity {symbol.MinArguments}.");
            CheckReferences(symbol.EvidenceIds, evidence, $"symbol '{symbol.Id}' evidence", errors);
            CheckReferences(symbol.NativeFunctionIds, nativeFunctions, $"symbol '{symbol.Id}' native functions", errors);
            CheckReferences(symbol.NativeTypeIds, nativeTypes, $"symbol '{symbol.Id}' native types", errors);
        }

        foreach (NativeFunction function in document.NativeFunctions)
        {
            if (!string.Equals(function.BuildId, document.CurrentNativeBuild.Id, StringComparison.Ordinal))
                errors.Add($"Native function '{function.Id}' is not keyed to current build '{document.CurrentNativeBuild.Id}'.");
            CheckReferences(function.EvidenceIds, evidence, $"native function '{function.Id}' evidence", errors);
        }
        foreach (NativeType type in document.NativeTypes)
        {
            if (!string.Equals(type.BuildId, document.CurrentNativeBuild.Id, StringComparison.Ordinal))
                errors.Add($"Native type '{type.Id}' is not keyed to current build '{document.CurrentNativeBuild.Id}'.");
            CheckReferences(type.EvidenceIds, evidence, $"native type '{type.Id}' evidence", errors);
        }
        foreach (NegativeFinding finding in document.NegativeFindings)
        {
            if (finding.Result is not ("TRUE" or "FALSE" or "UNRESOLVED"))
                errors.Add($"Finding '{finding.Id}' has invalid result '{finding.Result}'.");
            CheckReferences(finding.EvidenceIds, evidence, $"finding '{finding.Id}' evidence", errors);
        }

        SdkStatistics expected = RecomputeStatistics(document);
        if (JsonSerializer.Serialize(expected) != JsonSerializer.Serialize(document.Statistics))
            errors.Add("Statistics do not match document contents.");
        if (document.Statistics.CatalogSymbols < 225 || document.Statistics.HighConfidenceCatalogSymbols != 175)
            errors.Add("Curated catalog coverage fell below the established 225/175 gate.");
        if (document.Statistics.CorpusCensusRows != 55_709)
            errors.Add($"Full corpus census changed from the established 55,709 rows to {document.Statistics.CorpusCensusRows}.");

        if (errors.Count != 0)
            throw new InvalidDataException("Semantic SDK validation failed:\n- " + string.Join("\n- ", errors));
    }

    public static SemanticSdkDocument ValidateFiles(string directory, Workspace workspace)
    {
        string sdkPath = Path.Combine(directory, "semantic-sdk.json");
        string manifestPath = Path.Combine(directory, "manifest.json");
        SemanticSdkDocument document = JsonSerializer.Deserialize(
            File.ReadAllText(sdkPath), SemanticSdkJsonContext.Default.SemanticSdkDocument)
            ?? throw new InvalidDataException("semantic-sdk.json deserialized to null.");
        SdkManifest manifest = JsonSerializer.Deserialize(
            File.ReadAllText(manifestPath), SemanticSdkJsonContext.Default.SdkManifest)
            ?? throw new InvalidDataException("manifest.json deserialized to null.");
        ValidateDocument(document);

        if (!string.Equals(manifest.Generator, document.Generator, StringComparison.Ordinal))
            throw new InvalidDataException("Manifest generator differs from SDK generator.");
        foreach (SourceFingerprint source in manifest.Sources)
        {
            string fullPath = Path.GetFullPath(Path.Combine(workspace.Root, source.Path.Replace('/', Path.DirectorySeparatorChar)));
            VerifyFingerprint(fullPath, source.Sha256, source.Length, "source " + source.Id);
        }
        foreach (OutputFingerprint output in manifest.Outputs)
            VerifyFingerprint(Path.Combine(directory, output.Path), output.Sha256, output.Length, "output " + output.Path);
        return document;
    }

    public static SdkStatistics RecomputeStatistics(SemanticSdkDocument document) => new()
    {
        Symbols = document.Symbols.Count,
        DeepContracts = document.Symbols.Count(symbol => symbol.SourceKinds.Contains("DEEP_CONTRACT", StringComparer.Ordinal)),
        CatalogSymbols = document.Symbols.Count(symbol => symbol.SourceKinds.Contains("CORPUS_CATALOG", StringComparer.Ordinal)),
        HighConfidenceCatalogSymbols = document.Symbols.Count(symbol => symbol.Labels.Contains("HIGH_CONFIDENCE", StringComparer.Ordinal)),
        CorpusCallSites = document.Symbols.Where(symbol => symbol.Corpus is not null).Sum(symbol => (long)symbol.Corpus!.Sites),
        CorpusCensusRows = document.Statistics.CorpusCensusRows,
        NativeFunctions = document.NativeFunctions.Count,
        NativeTypes = document.NativeTypes.Count,
        ExplicitNativeLinks = document.Symbols.Sum(symbol => symbol.NativeFunctionIds.Count + symbol.NativeTypeIds.Count),
        EvidenceRecords = document.Evidence.Count,
        NegativeFindings = document.NegativeFindings.Count,
    };

    private static void VerifyFingerprint(string path, string expectedHash, long expectedLength, string label)
    {
        byte[] bytes = File.ReadAllBytes(path);
        string actualHash = Convert.ToHexString(SHA256.HashData(bytes));
        if (bytes.LongLength != expectedLength || !string.Equals(actualHash, expectedHash, StringComparison.Ordinal))
            throw new InvalidDataException($"Fingerprint mismatch for {label}: expected {expectedLength}/{expectedHash}, got {bytes.LongLength}/{actualHash}.");
    }

    private static void RequireUnique(IEnumerable<string> values, string label, List<string> errors)
    {
        foreach (IGrouping<string, string> duplicate in values.GroupBy(value => value, StringComparer.Ordinal).Where(group => group.Count() > 1))
            errors.Add($"Duplicate {label}: '{duplicate.Key}'.");
    }

    private static void CheckReferences(IEnumerable<string> values, IReadOnlySet<string> allowed, string label, List<string> errors)
    {
        foreach (string value in values)
            if (!allowed.Contains(value)) errors.Add($"Unknown {label} reference '{value}'.");
    }

    [GeneratedRegex("^0x[0-9a-fA-F]{8}$", RegexOptions.CultureInvariant)]
    private static partial Regex NameHash();
}
