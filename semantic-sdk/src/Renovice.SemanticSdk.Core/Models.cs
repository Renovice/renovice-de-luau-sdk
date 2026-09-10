using System.Text.Json.Serialization;

namespace Renovice.SemanticSdk.Core;

public sealed class SemanticSdkDocument
{
    public int SchemaVersion { get; init; } = 1;
    public required string Generator { get; init; }
    public required NativeBuild CurrentNativeBuild { get; init; }
    public required List<SourceFingerprint> Sources { get; init; }
    public required List<ApiSymbol> Symbols { get; init; }
    public required List<NativeFunction> NativeFunctions { get; init; }
    public required List<NativeType> NativeTypes { get; init; }
    public required List<EvidenceRecord> Evidence { get; init; }
    public required List<NegativeFinding> NegativeFindings { get; init; }
    public required SdkStatistics Statistics { get; init; }
}

public sealed class SourceFingerprint
{
    public required string Id { get; init; }
    public required string Path { get; init; }
    public required string Sha256 { get; init; }
    public long Length { get; init; }
}

public sealed class ApiSymbol
{
    public required string Id { get; init; }
    public required string Kind { get; init; }
    public required string Owner { get; init; }
    public required string Name { get; init; }
    public string? Hash { get; set; }
    public int? MinArguments { get; set; }
    public int? MaxArguments { get; set; }
    public required List<SemanticParameter> Parameters { get; init; }
    public required List<string> Returns { get; init; }
    public required List<string> Labels { get; init; }
    public required string Confidence { get; set; }
    public required string Status { get; set; }
    public string? Authority { get; set; }
    public string? Lifetime { get; set; }
    public string? CallbackArity { get; set; }
    public string? CallbackParameters { get; set; }
    public string? Notes { get; set; }
    public required List<string> EvidenceIds { get; init; }
    public required List<string> SourceKinds { get; init; }
    public CorpusObservation? Corpus { get; set; }
    public required List<string> NativeFunctionIds { get; init; }
    public required List<string> NativeTypeIds { get; init; }
}

public sealed class SemanticParameter
{
    public required string Name { get; init; }
    public required string Type { get; init; }
}

public sealed class CorpusObservation
{
    public int Sites { get; init; }
    public int Modules { get; init; }
    public int Prototypes { get; init; }
    public required List<string> VisibleArgumentShapes { get; init; }
    public required List<int> ObservedResultShapes { get; init; }
    public required List<string> ConsumptionCategories { get; init; }
    public required string EvidenceGrade { get; init; }
    public required string Rationale { get; init; }
}

public sealed class NativeBuild
{
    public required string Id { get; init; }
    public required string ProductVersion { get; init; }
    public required string Sha256 { get; init; }
    public long Length { get; init; }
    public required string Architecture { get; init; }
    public required string Status { get; init; }
}

public sealed class NativeFunction
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string BuildId { get; init; }
    public required string Status { get; init; }
    public required string Confidence { get; init; }
    public string? Rva { get; init; }
    public required string Signature { get; init; }
    public required string Summary { get; init; }
    public required string Arguments { get; init; }
    public required string ReturnContract { get; init; }
    public required string SideEffects { get; init; }
    public required List<string> EvidenceIds { get; init; }
    public required List<string> Unresolved { get; init; }
}

public sealed class NativeType
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string BuildId { get; init; }
    public required string Status { get; init; }
    public required string Confidence { get; init; }
    public int? Size { get; init; }
    public required string Summary { get; init; }
    public required List<NativeField> Fields { get; init; }
    public required List<string> EvidenceIds { get; init; }
    public required List<string> Unresolved { get; init; }
}

public sealed class NativeField
{
    public required string Name { get; init; }
    public int? Offset { get; init; }
    public int? Size { get; init; }
    public required string Type { get; init; }
    public required string Status { get; init; }
}

public sealed class EvidenceRecord
{
    public required string Id { get; init; }
    public required string Domain { get; init; }
    public required string Kind { get; init; }
    public required string Result { get; init; }
    public required string Confidence { get; init; }
    public required string Source { get; init; }
    public required string Observed { get; init; }
    public required string Limitations { get; init; }
    public required List<string> BuildIds { get; init; }
}

public sealed class NegativeFinding
{
    public required string Id { get; init; }
    public required string Domain { get; init; }
    public required string Hypothesis { get; init; }
    public required string Result { get; init; }
    public required string Conclusion { get; init; }
    public required List<string> EvidenceIds { get; init; }
}

public sealed class SdkStatistics
{
    public int Symbols { get; init; }
    public int DeepContracts { get; init; }
    public int CatalogSymbols { get; init; }
    public int HighConfidenceCatalogSymbols { get; init; }
    public long CorpusCallSites { get; init; }
    public int CorpusCensusRows { get; init; }
    public int NativeFunctions { get; init; }
    public int NativeTypes { get; init; }
    public int ExplicitNativeLinks { get; init; }
    public int EvidenceRecords { get; init; }
    public int NegativeFindings { get; init; }
}

public sealed class SdkManifest
{
    public int SchemaVersion { get; init; } = 1;
    public required string Generator { get; init; }
    public required List<SourceFingerprint> Sources { get; init; }
    public required List<OutputFingerprint> Outputs { get; init; }
}

public sealed class OutputFingerprint
{
    public required string Path { get; init; }
    public required string Sha256 { get; init; }
    public long Length { get; init; }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SemanticSdkDocument))]
[JsonSerializable(typeof(SdkManifest))]
internal sealed partial class SemanticSdkJsonContext : JsonSerializerContext;
