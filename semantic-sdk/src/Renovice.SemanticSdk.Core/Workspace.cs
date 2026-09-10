using System.Security.Cryptography;
using System.Text.Json;

namespace Renovice.SemanticSdk.Core;

public sealed class Workspace
{
    public required string Root { get; init; }
    public required string DeLuauToolchain { get; init; }
    public required string NativeAnalysis { get; init; }
    public required string Research { get; init; }
    public required string SharedSemanticSdk { get; init; }

    public static Workspace Resolve(string? explicitRoot = null)
    {
        string? root = explicitRoot is null
            ? FindRoot(Environment.CurrentDirectory) ?? FindRoot(AppContext.BaseDirectory)
            : Path.GetFullPath(explicitRoot);
        if (root is null)
            throw new DirectoryNotFoundException("Could not locate WORKSPACE.json.");

        string manifestPath = Path.Combine(root, "WORKSPACE.json");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(manifestPath));
        JsonElement manifest = document.RootElement;
        JsonElement repos = manifest.GetProperty("repos");
        JsonElement shared = manifest.GetProperty("shared");
        JsonElement protectedPaths = manifest.GetProperty("protected");
        return new Workspace
        {
            Root = root,
            DeLuauToolchain = ResolvePath(root, repos.GetProperty("de_luau_toolchain").GetString()),
            NativeAnalysis = ResolvePath(root, repos.GetProperty("native_analysis").GetString()),
            Research = ResolvePath(root, protectedPaths.GetProperty("research").GetString()),
            SharedSemanticSdk = ResolvePath(root, shared.GetProperty("semantic_sdk").GetString()),
        };
    }

    private static string? FindRoot(string start)
    {
        var directory = new DirectoryInfo(Path.GetFullPath(start));
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WORKSPACE.json")))
                return directory.FullName;
            directory = directory.Parent;
        }
        return null;
    }

    private static string ResolvePath(string root, string? relative)
    {
        if (string.IsNullOrWhiteSpace(relative))
            throw new InvalidDataException("WORKSPACE.json contains an empty authoritative path.");
        return Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
    }

    public string Relative(string path) => Path.GetRelativePath(Root, path).Replace('\\', '/');

    public SourceFingerprint Fingerprint(string id, string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        return new SourceFingerprint
        {
            Id = id,
            Path = Relative(path),
            Sha256 = Convert.ToHexString(SHA256.HashData(bytes)),
            Length = bytes.LongLength,
        };
    }
}
