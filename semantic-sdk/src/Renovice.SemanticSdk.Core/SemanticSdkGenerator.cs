using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Renovice.SemanticSdk.Core;

public static class SemanticSdkGenerator
{
    public const string GeneratorIdentity = "RENOVICE_SEMANTIC_SDK_V1";

    public static SemanticSdkDocument Generate(Workspace workspace, string semanticSdkToolchain)
    {
        string api = Path.Combine(workspace.DeLuauToolchain, "api", "warframe");
        string contractsPath = Path.Combine(api, "contracts.tsv");
        string catalogPath = Path.Combine(api, "selected_catalog.tsv");
        string evidencePath = Path.Combine(api, "evidence.tsv");
        string negativePath = Path.Combine(api, "negative_contracts.tsv");
        string nativeRegistryPath = Path.Combine(workspace.NativeAnalysis, "registry", "registry.json");
        string linksPath = Path.Combine(semanticSdkToolchain, "registry", "native_links.tsv");
        string censusPath = Path.Combine(workspace.Research, "DE LUAU TRANSLATOR",
            "NATIVE API AND LIVE CANDIDATE CENSUS", "result_consumption_sites.tsv");

        var sources = new List<SourceFingerprint>
        {
            workspace.Fingerprint("lua-contracts", contractsPath),
            workspace.Fingerprint("lua-corpus-catalog", catalogPath),
            workspace.Fingerprint("lua-evidence", evidencePath),
            workspace.Fingerprint("lua-negative-contracts", negativePath),
            workspace.Fingerprint("native-registry", nativeRegistryPath),
            workspace.Fingerprint("explicit-native-links", linksPath),
            workspace.Fingerprint("lua-full-callsite-census", censusPath),
        };
        int censusRows = TsvTable.Load(censusPath).Rows.Count;

        var builders = LoadCatalog(catalogPath);
        MergeContracts(contractsPath, builders);
        List<EvidenceRecord> evidence = LoadLuaEvidence(evidencePath);
        List<NegativeFinding> negatives = LoadLuaNegativeFindings(negativePath);

        NativeRegistryData native = LoadNativeRegistry(nativeRegistryPath);
        evidence.AddRange(native.Evidence);
        negatives.AddRange(native.NegativeFindings);

        ApplyExplicitLinks(linksPath, builders, native, evidence);

        List<ApiSymbol> symbols = builders.Values
            .Select(builder => builder.Finish())
            .OrderBy(symbol => symbol.Id, StringComparer.Ordinal)
            .ToList();
        evidence = evidence.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        negatives = negatives.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        sources = sources.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();

        var document = new SemanticSdkDocument
        {
            Generator = GeneratorIdentity,
            CurrentNativeBuild = native.CurrentBuild,
            Sources = sources,
            Symbols = symbols,
            NativeFunctions = native.Functions.OrderBy(item => item.Id, StringComparer.Ordinal).ToList(),
            NativeTypes = native.Types.OrderBy(item => item.Id, StringComparer.Ordinal).ToList(),
            Evidence = evidence,
            NegativeFindings = negatives,
            Statistics = new SdkStatistics
            {
                Symbols = symbols.Count,
                DeepContracts = symbols.Count(symbol => symbol.SourceKinds.Contains("DEEP_CONTRACT", StringComparer.Ordinal)),
                CatalogSymbols = symbols.Count(symbol => symbol.SourceKinds.Contains("CORPUS_CATALOG", StringComparer.Ordinal)),
                HighConfidenceCatalogSymbols = symbols.Count(symbol => symbol.Labels.Contains("HIGH_CONFIDENCE", StringComparer.Ordinal)),
                CorpusCallSites = symbols.Where(symbol => symbol.Corpus is not null).Sum(symbol => (long)symbol.Corpus!.Sites),
                CorpusCensusRows = censusRows,
                NativeFunctions = native.Functions.Count,
                NativeTypes = native.Types.Count,
                ExplicitNativeLinks = symbols.Sum(symbol => symbol.NativeFunctionIds.Count + symbol.NativeTypeIds.Count),
                EvidenceRecords = evidence.Count,
                NegativeFindings = negatives.Count,
            },
        };
        SemanticSdkValidator.ValidateDocument(document);
        return document;
    }

    public static SdkManifest Publish(Workspace workspace, string semanticSdkToolchain)
    {
        SemanticSdkDocument document = Generate(workspace, semanticSdkToolchain);
        string outputDirectory = workspace.SharedSemanticSdk;
        Directory.CreateDirectory(outputDirectory);
        string staging = Path.Combine(outputDirectory, ".staging-" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(staging);
        try
        {
            string sdkPath = Path.Combine(staging, "semantic-sdk.json");
            string symbolsPath = Path.Combine(staging, "symbols.tsv");
            File.WriteAllText(sdkPath, SerializeDocument(document), new UTF8Encoding(false));
            File.WriteAllText(symbolsPath, SerializeSymbols(document.Symbols), new UTF8Encoding(false));

            var manifest = new SdkManifest
            {
                Generator = GeneratorIdentity,
                Sources = document.Sources,
                Outputs = new List<OutputFingerprint>
                {
                    FingerprintOutput("semantic-sdk.json", sdkPath),
                    FingerprintOutput("symbols.tsv", symbolsPath),
                },
            };
            string manifestPath = Path.Combine(staging, "manifest.json");
            File.WriteAllText(manifestPath, SerializeManifest(manifest), new UTF8Encoding(false));
            SemanticSdkValidator.ValidateFiles(staging, workspace);

            foreach (string name in new[] { "semantic-sdk.json", "symbols.tsv", "manifest.json" })
                File.Move(Path.Combine(staging, name), Path.Combine(outputDirectory, name), true);
            return manifest;
        }
        finally
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
        }
    }

    public static string SerializeDocument(SemanticSdkDocument document) =>
        JsonSerializer.Serialize(document, SemanticSdkJsonContext.Default.SemanticSdkDocument) + "\n";

    public static string SerializeManifest(SdkManifest manifest) =>
        JsonSerializer.Serialize(manifest, SemanticSdkJsonContext.Default.SdkManifest) + "\n";

    public static string SerializeSymbols(IEnumerable<ApiSymbol> symbols)
    {
        var output = new StringBuilder();
        output.Append("schema_version\tgenerator\tid\tkind\towner\tname\thash\tmin_args\tmax_args\tobserved_args\tobserved_open_args\tparameters\treturns\tconfidence\tstatus\tevidence\tsource_kinds\tsites\tmodules\tprototypes\tnative_function_ids\tnative_type_ids\n");
        foreach (ApiSymbol symbol in symbols.OrderBy(item => item.Id, StringComparer.Ordinal))
        {
            string parameters = string.Join(';', symbol.Parameters.Select(item => item.Name + ":" + item.Type));
            List<int> observedArguments = symbol.Corpus?.VisibleArgumentShapes
                .Select(value => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                    && parsed >= 0 ? (int?)parsed : null)
                .Where(value => value is not null)
                .Select(value => value!.Value)
                .Distinct()
                .OrderBy(value => value)
                .ToList() ?? new List<int>();
            bool observedOpenArguments = symbol.Corpus?.VisibleArgumentShapes
                .Contains("open", StringComparer.Ordinal) ?? false;
            output.AppendJoin('\t', new[]
            {
                "1", GeneratorIdentity,
                Escape(symbol.Id), Escape(symbol.Kind), Escape(symbol.Owner), Escape(symbol.Name), Escape(symbol.Hash),
                Invariant(symbol.MinArguments), Invariant(symbol.MaxArguments),
                Escape(string.Join(';', observedArguments)), observedOpenArguments ? "true" : "false",
                Escape(parameters),
                Escape(string.Join(';', symbol.Returns)), Escape(symbol.Confidence), Escape(symbol.Status),
                Escape(string.Join(';', symbol.EvidenceIds)), Escape(string.Join(';', symbol.SourceKinds)),
                Invariant(symbol.Corpus?.Sites), Invariant(symbol.Corpus?.Modules), Invariant(symbol.Corpus?.Prototypes),
                Escape(string.Join(';', symbol.NativeFunctionIds)), Escape(string.Join(';', symbol.NativeTypeIds)),
            });
            output.Append('\n');
        }
        return output.ToString();
    }

    private static Dictionary<string, SymbolBuilder> LoadCatalog(string path)
    {
        TsvTable table = TsvTable.Load(path, "kind", "owner_hint", "name", "hash", "sites", "modules", "prototypes");
        var symbols = new Dictionary<string, SymbolBuilder>(StringComparer.Ordinal);
        foreach (TsvRow row in table.Rows)
        {
            string kind = row.Get("kind");
            string owner = row.Get("owner_hint");
            string name = row.Get("name");
            string key = SymbolKey(kind, owner, name);
            if (symbols.ContainsKey(key))
                throw new InvalidDataException($"Duplicate catalog identity: {key}");

            List<string> visibleArguments = StringSet(row.Get("visible_args"), '|');
            List<int> nonnegativeArguments = visibleArguments
                .Select(value => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                    ? (int?)parsed : null)
                .Where(value => value >= 0).Select(value => value!.Value).ToList();
            var builder = new SymbolBuilder(kind, owner, name)
            {
                Hash = EmptyToNull(row.Get("hash")),
                MinArguments = nonnegativeArguments.Count == 0 ? null : nonnegativeArguments.Min(),
                MaxArguments = nonnegativeArguments.Count == 0 ? null : nonnegativeArguments.Max(),
                Confidence = row.Get("evidence_grade"),
                Status = "CATALOG_ONLY",
                Corpus = new CorpusObservation
                {
                    Sites = row.GetInt("sites"),
                    Modules = row.GetInt("modules"),
                    Prototypes = row.GetInt("prototypes"),
                    VisibleArgumentShapes = visibleArguments,
                    ObservedResultShapes = IntegerSet(row.Get("observed_results")),
                    ConsumptionCategories = StringSet(row.Get("consumption_categories"), '|'),
                    EvidenceGrade = row.Get("evidence_grade"),
                    Rationale = row.Get("rationale"),
                },
            };
            builder.Labels.AddRange(StringSet(row.Get("labels"), ';'));
            string result = row.Get("return_family_hint");
            if (!IsAbsent(result)) builder.Returns.Add(result);
            builder.SourceKinds.Add("CORPUS_CATALOG");
            symbols.Add(key, builder);
        }
        return symbols;
    }

    private static void MergeContracts(string path, Dictionary<string, SymbolBuilder> symbols)
    {
        TsvTable table = TsvTable.Load(path, "kind", "owner", "name", "min_args", "max_args", "parameters", "returns", "confidence", "status", "evidence");
        foreach (TsvRow row in table.Rows)
        {
            string kind = row.Get("kind");
            string owner = row.Get("owner");
            string name = row.Get("name");
            string key = SymbolKey(kind, owner, name);
            if (!symbols.TryGetValue(key, out SymbolBuilder? builder))
            {
                builder = new SymbolBuilder(kind, owner, name);
                symbols.Add(key, builder);
            }
            if (builder.SourceKinds.Contains("DEEP_CONTRACT", StringComparer.Ordinal))
                throw new InvalidDataException($"Duplicate deep contract identity: {key}");

            builder.MinArguments = ParseOptionalInt(row.Get("min_args"));
            builder.MaxArguments = ParseOptionalInt(row.Get("max_args"));
            builder.Parameters.Clear();
            builder.Parameters.AddRange(ParseParameters(
                row.Get("parameters"), string.Equals(kind, "field", StringComparison.Ordinal)));
            builder.Returns.Clear();
            builder.Returns.AddRange(ReturnTypes(row.Get("returns")));
            builder.Confidence = row.Get("confidence");
            builder.Status = row.Get("status");
            builder.Authority = EmptyToNull(row.Get("authority"));
            builder.Lifetime = EmptyToNull(row.Get("lifetime"));
            builder.CallbackArity = EmptyToNull(row.Get("callback_arity"));
            builder.CallbackParameters = EmptyToNull(row.Get("callback_parameters"));
            builder.Notes = EmptyToNull(row.Get("notes"));
            builder.EvidenceIds.AddRange(StringSet(row.Get("evidence"), ';'));
            builder.SourceKinds.Add("DEEP_CONTRACT");
        }
    }

    private static List<EvidenceRecord> LoadLuaEvidence(string path)
    {
        TsvTable table = TsvTable.Load(path, "evidence_id", "kind", "result", "source", "observed", "limitations");
        return table.Rows.Select(row => new EvidenceRecord
        {
            Id = row.Get("evidence_id"), Domain = "LUA_API", Kind = row.Get("kind"),
            Result = row.Get("result"), Confidence = ConfidenceFromEvidenceKind(row.Get("kind")),
            Source = row.Get("source"), Observed = row.Get("observed"), Limitations = row.Get("limitations"),
            BuildIds = new List<string>(),
        }).ToList();
    }

    private static List<NegativeFinding> LoadLuaNegativeFindings(string path)
    {
        TsvTable table = TsvTable.Load(path, "hypothesis", "result", "evidence_id", "consequence");
        int index = 0;
        return table.Rows.Select(row => new NegativeFinding
        {
            Id = "lua-negative-" + (++index).ToString("D4", CultureInfo.InvariantCulture),
            Domain = "LUA_API", Hypothesis = row.Get("hypothesis"), Result = row.Get("result"),
            Conclusion = row.Get("consequence"), EvidenceIds = StringSet(row.Get("evidence_id"), ';'),
        }).ToList();
    }

    private static NativeRegistryData LoadNativeRegistry(string path)
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(path));
        JsonElement root = document.RootElement;
        string currentBuildId = root.GetProperty("currentBuildId").GetString()
            ?? throw new InvalidDataException("Native registry currentBuildId is null.");
        NativeBuild? currentBuild = null;
        foreach (JsonElement item in root.GetProperty("builds").EnumerateArray())
        {
            if (!string.Equals(GetRequiredString(item, "id"), currentBuildId, StringComparison.Ordinal)) continue;
            currentBuild = new NativeBuild
            {
                Id = currentBuildId,
                ProductVersion = GetRequiredString(item, "productVersion"),
                Sha256 = GetRequiredString(item, "sha256"),
                Length = item.GetProperty("length").GetInt64(),
                Architecture = GetRequiredString(item, "architecture"),
                Status = GetRequiredString(item, "status"),
            };
        }
        if (currentBuild is null)
            throw new InvalidDataException($"Native registry current build '{currentBuildId}' is absent.");

        var functions = new List<NativeFunction>();
        foreach (JsonElement item in root.GetProperty("functions").EnumerateArray())
        {
            functions.Add(new NativeFunction
            {
                Id = GetRequiredString(item, "id"), Name = GetRequiredString(item, "name"),
                BuildId = GetRequiredString(item, "buildId"), Status = GetRequiredString(item, "status"),
                Confidence = GetRequiredString(item, "confidence"), Rva = OptionalScalar(item, "rva"),
                Signature = GetRequiredString(item, "signature"), Summary = GetRequiredString(item, "summary"),
                Arguments = GetRequiredString(item, "arguments"), ReturnContract = GetRequiredString(item, "returnContract"),
                SideEffects = GetRequiredString(item, "sideEffects"), EvidenceIds = StringArray(item, "evidenceIds"),
                Unresolved = StringArray(item, "unresolved"),
            });
        }

        var types = new List<NativeType>();
        foreach (JsonElement item in root.GetProperty("types").EnumerateArray())
        {
            var fields = new List<NativeField>();
            foreach (JsonElement field in item.GetProperty("fields").EnumerateArray())
            {
                fields.Add(new NativeField
                {
                    Name = GetRequiredString(field, "name"), Offset = OptionalInt(field, "offset"),
                    Size = OptionalInt(field, "size"), Type = GetRequiredString(field, "type"),
                    Status = GetRequiredString(field, "status"),
                });
            }
            types.Add(new NativeType
            {
                Id = GetRequiredString(item, "id"), Name = GetRequiredString(item, "name"),
                BuildId = GetRequiredString(item, "buildId"), Status = GetRequiredString(item, "status"),
                Confidence = GetRequiredString(item, "confidence"), Size = OptionalInt(item, "size"),
                Summary = GetRequiredString(item, "summary"), Fields = fields.OrderBy(field => field.Offset).ThenBy(field => field.Name, StringComparer.Ordinal).ToList(),
                EvidenceIds = StringArray(item, "evidenceIds"), Unresolved = StringArray(item, "unresolved"),
            });
        }

        var evidence = new List<EvidenceRecord>();
        foreach (JsonElement item in root.GetProperty("evidence").EnumerateArray())
        {
            evidence.Add(new EvidenceRecord
            {
                Id = GetRequiredString(item, "id"), Domain = "NATIVE", Kind = GetRequiredString(item, "kind"),
                Result = "RECORDED", Confidence = GetRequiredString(item, "confidence"),
                Source = GetRequiredString(item, "sourcePath"), Observed = GetRequiredString(item, "summary"),
                Limitations = string.Empty, BuildIds = StringArray(item, "buildIds"),
            });
        }

        var negatives = new List<NegativeFinding>();
        foreach (JsonElement item in root.GetProperty("findings").EnumerateArray())
        {
            string result = GetRequiredString(item, "result");
            if (!string.Equals(result, "TRUE", StringComparison.Ordinal)
                && !string.Equals(result, "FALSE", StringComparison.Ordinal)
                && !string.Equals(result, "UNRESOLVED", StringComparison.Ordinal))
                throw new InvalidDataException($"Unexpected native finding result '{result}'.");
            negatives.Add(new NegativeFinding
            {
                Id = GetRequiredString(item, "id"), Domain = "NATIVE",
                Hypothesis = GetRequiredString(item, "hypothesis"), Result = result,
                Conclusion = GetRequiredString(item, "conclusion"), EvidenceIds = StringArray(item, "evidenceIds"),
            });
        }
        return new NativeRegistryData(currentBuild, functions, types, evidence, negatives);
    }

    private static void ApplyExplicitLinks(string path, Dictionary<string, SymbolBuilder> symbols,
        NativeRegistryData native, IReadOnlyList<EvidenceRecord> evidence)
    {
        TsvTable table = TsvTable.Load(path, "lua_symbol_id", "native_function_id", "native_type_id", "evidence_id", "status");
        var byId = symbols.Values.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var functions = native.Functions.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var types = native.Types.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var evidenceIds = evidence.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        foreach (TsvRow row in table.Rows)
        {
            string symbolId = row.Get("lua_symbol_id");
            if (!byId.TryGetValue(symbolId, out SymbolBuilder? symbol))
                throw new InvalidDataException($"Explicit link references unknown Lua symbol '{symbolId}'.");
            string functionId = row.Get("native_function_id");
            string typeId = row.Get("native_type_id");
            string evidenceId = row.Get("evidence_id");
            if (!IsAbsent(functionId) && !functions.Contains(functionId))
                throw new InvalidDataException($"Explicit link references unknown native function '{functionId}'.");
            if (!IsAbsent(typeId) && !types.Contains(typeId))
                throw new InvalidDataException($"Explicit link references unknown native type '{typeId}'.");
            if (!IsAbsent(evidenceId) && !evidenceIds.Contains(evidenceId))
                throw new InvalidDataException($"Explicit link references unknown evidence '{evidenceId}'.");
            if (!string.Equals(row.Get("status"), "CONFIRMED", StringComparison.Ordinal))
                continue;
            if (!IsAbsent(functionId)) symbol.NativeFunctionIds.Add(functionId);
            if (!IsAbsent(typeId)) symbol.NativeTypeIds.Add(typeId);
            if (!IsAbsent(evidenceId)) symbol.EvidenceIds.Add(evidenceId);
            symbol.SourceKinds.Add("EXPLICIT_NATIVE_LINK");
        }
    }

    private static OutputFingerprint FingerprintOutput(string relative, string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        return new OutputFingerprint
        {
            Path = relative, Sha256 = Convert.ToHexString(SHA256.HashData(bytes)), Length = bytes.LongLength,
        };
    }

    private static string SymbolKey(string kind, string owner, string name) => kind + "\u001f" + owner + "\u001f" + name;
    public static string SymbolId(string kind, string owner, string name) =>
        "lua:" + Uri.EscapeDataString(kind) + ":" + Uri.EscapeDataString(owner) + ":" + Uri.EscapeDataString(name);

    private static List<SemanticParameter> ParseParameters(string value, bool fieldValue = false)
    {
        if (IsAbsent(value)) return new List<SemanticParameter>();
        var result = new List<SemanticParameter>();
        foreach (string item in value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            int colon = item.IndexOf(':');
            result.Add(new SemanticParameter
            {
                Name = colon < 0 && fieldValue ? "value" : colon < 0 ? item : item[..colon].Trim(),
                Type = colon < 0 && fieldValue ? item : colon < 0 ? "unknown" : item[(colon + 1)..].Trim(),
            });
        }
        return result;
    }

    private static List<string> ReturnTypes(string value) => IsAbsent(value)
        ? new List<string>()
        : value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static List<int> IntegerSet(string value)
    {
        if (IsAbsent(value)) return new List<int>();
        return value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => int.Parse(item, NumberStyles.Integer, CultureInfo.InvariantCulture))
            .Distinct().Order().ToList();
    }

    private static List<string> StringSet(string value, char delimiter) => IsAbsent(value)
        ? new List<string>()
        : value.Split(delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();

    private static int? ParseOptionalInt(string value) => IsAbsent(value) ? null
        : int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
    private static bool IsAbsent(string? value) => string.IsNullOrWhiteSpace(value) || value == "-";
    private static string? EmptyToNull(string? value) => IsAbsent(value) ? null : value;
    private static string Escape(string? value) => (value ?? string.Empty).Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
    private static string Invariant(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    private static string ConfidenceFromEvidenceKind(string kind) => kind switch
    {
        "live_runtime" => "LIVE_CONFIRMED",
        "native_binding" => "STATIC_CONTRACT",
        "stock_bytecode" => "STOCK_BYTECODE",
        "offline_differential" => "OFFLINE_FIXTURE",
        _ => "RECORDED",
    };

    private static string GetRequiredString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out JsonElement value) || value.ValueKind != JsonValueKind.String)
            throw new InvalidDataException($"Native registry property '{property}' is missing or not a string.");
        return value.GetString()!;
    }

    private static string? OptionalScalar(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out JsonElement value) || value.ValueKind == JsonValueKind.Null) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText();
    }

    private static int? OptionalInt(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out JsonElement value) || value.ValueKind == JsonValueKind.Null) return null;
        return value.GetInt32();
    }

    private static List<string> StringArray(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out JsonElement value) || value.ValueKind == JsonValueKind.Null)
            return new List<string>();
        return value.EnumerateArray().Select(item => item.GetString()
            ?? throw new InvalidDataException($"Native registry array '{property}' contains a non-string."))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
    }

    private sealed record NativeRegistryData(
        NativeBuild CurrentBuild,
        List<NativeFunction> Functions,
        List<NativeType> Types,
        List<EvidenceRecord> Evidence,
        List<NegativeFinding> NegativeFindings);

    private sealed class SymbolBuilder
    {
        public string Id { get; }
        public string Kind { get; }
        public string Owner { get; }
        public string Name { get; }
        public string? Hash { get; set; }
        public int? MinArguments { get; set; }
        public int? MaxArguments { get; set; }
        public List<SemanticParameter> Parameters { get; } = new();
        public List<string> Returns { get; } = new();
        public List<string> Labels { get; } = new();
        public string Confidence { get; set; } = "UNRESOLVED";
        public string Status { get; set; } = "UNRESOLVED";
        public string? Authority { get; set; }
        public string? Lifetime { get; set; }
        public string? CallbackArity { get; set; }
        public string? CallbackParameters { get; set; }
        public string? Notes { get; set; }
        public List<string> EvidenceIds { get; } = new();
        public List<string> SourceKinds { get; } = new();
        public CorpusObservation? Corpus { get; set; }
        public List<string> NativeFunctionIds { get; } = new();
        public List<string> NativeTypeIds { get; } = new();

        public SymbolBuilder(string kind, string owner, string name)
        {
            Kind = kind; Owner = owner; Name = name; Id = SymbolId(kind, owner, name);
        }

        public ApiSymbol Finish() => new()
        {
            Id = Id, Kind = Kind, Owner = Owner, Name = Name, Hash = Hash,
            MinArguments = MinArguments, MaxArguments = MaxArguments,
            Parameters = Parameters.ToList(), Returns = Returns.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Labels = Labels.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Confidence = Confidence, Status = Status, Authority = Authority, Lifetime = Lifetime,
            CallbackArity = CallbackArity, CallbackParameters = CallbackParameters, Notes = Notes,
            EvidenceIds = EvidenceIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            SourceKinds = SourceKinds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Corpus = Corpus,
            NativeFunctionIds = NativeFunctionIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            NativeTypeIds = NativeTypeIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
        };
    }
}
