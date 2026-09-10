using System.Text.Json;
using Renovice.SemanticSdk.Core;

static int Usage()
{
    Console.Error.WriteLine("Usage: renovice-semantic <selftest|build|validate|query> [value] [--workspace <root>]");
    return 2;
}

static string? Option(IReadOnlyList<string> arguments, string name)
{
    for (int index = 0; index < arguments.Count; index++)
        if (string.Equals(arguments[index], name, StringComparison.Ordinal))
            return index + 1 < arguments.Count
                ? arguments[index + 1]
                : throw new ArgumentException($"Missing value for {name}.");
    return null;
}

try
{
    if (args.Length == 0) return Usage();
    string command = args[0];
    Workspace workspace = Workspace.Resolve(Option(args, "--workspace"));
    string toolchainRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ".."));
    if (!File.Exists(Path.Combine(toolchainRoot, "registry", "native_links.tsv")))
    {
        string? discovered = Directory.EnumerateDirectories(Path.Combine(workspace.Root, "repos", "toolchains"), "semantic-sdk")
            .SingleOrDefault();
        toolchainRoot = discovered ?? throw new DirectoryNotFoundException("Could not locate semantic-sdk toolchain root.");
    }

    switch (command)
    {
        case "selftest":
        {
            IReadOnlyList<string> passed = SemanticSdkSelfTest.Run(workspace, toolchainRoot);
            foreach (string item in passed) Console.WriteLine("PASS " + item);
            Console.WriteLine($"RESULT PASS tests={passed.Count} failures=0");
            return 0;
        }
        case "build":
        {
            SdkManifest manifest = SemanticSdkGenerator.Publish(workspace, toolchainRoot);
            SemanticSdkDocument document = SemanticSdkValidator.ValidateFiles(workspace.SharedSemanticSdk, workspace);
            Console.WriteLine($"BUILD PASS symbols={document.Statistics.Symbols} deep={document.Statistics.DeepContracts} "
                + $"catalog={document.Statistics.CatalogSymbols} sites={document.Statistics.CorpusCallSites} "
                + $"native_functions={document.Statistics.NativeFunctions} native_types={document.Statistics.NativeTypes}");
            foreach (OutputFingerprint output in manifest.Outputs)
                Console.WriteLine($"OUTPUT {output.Path} bytes={output.Length} sha256={output.Sha256}");
            return 0;
        }
        case "validate":
        {
            SemanticSdkDocument document = SemanticSdkValidator.ValidateFiles(workspace.SharedSemanticSdk, workspace);
            Console.WriteLine($"VALIDATE PASS symbols={document.Statistics.Symbols} evidence={document.Statistics.EvidenceRecords} "
                + $"negative={document.Statistics.NegativeFindings} explicit_native_links={document.Statistics.ExplicitNativeLinks}");
            return 0;
        }
        case "query":
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal)) return Usage();
            SemanticSdkDocument document = SemanticSdkValidator.ValidateFiles(workspace.SharedSemanticSdk, workspace);
            string term = args[1];
            List<ApiSymbol> matches = document.Symbols.Where(symbol =>
                    symbol.Id.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || symbol.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (symbol.Hash?.Equals(term, StringComparison.OrdinalIgnoreCase) ?? false))
                .OrderBy(symbol => symbol.Id, StringComparer.Ordinal).ToList();
            Console.WriteLine(JsonSerializer.Serialize(matches, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            Console.WriteLine($"RESULT matches={matches.Count}");
            return matches.Count == 0 ? 1 : 0;
        }
        default:
            return Usage();
    }
}
catch (Exception exception)
{
    Console.Error.WriteLine("FAIL " + exception.Message);
    return 1;
}
