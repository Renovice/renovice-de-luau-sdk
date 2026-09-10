namespace Renovice.SemanticSdk.Core;

internal sealed class TsvTable
{
    private readonly Dictionary<string, int> _columns;
    public IReadOnlyList<TsvRow> Rows { get; }

    private TsvTable(Dictionary<string, int> columns, List<TsvRow> rows)
    {
        _columns = columns;
        Rows = rows;
    }

    public static TsvTable Load(string path, params string[] requiredColumns)
    {
        using var reader = new StreamReader(path);
        string? headerLine = reader.ReadLine();
        if (headerLine is null)
            throw new InvalidDataException($"TSV is empty: {path}");

        string[] headers = headerLine.TrimEnd('\r').Split('\t');
        var columns = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int index = 0; index < headers.Length; index++)
        {
            if (!columns.TryAdd(headers[index], index))
                throw new InvalidDataException($"Duplicate TSV column '{headers[index]}': {path}");
        }
        foreach (string required in requiredColumns)
            if (!columns.ContainsKey(required))
                throw new InvalidDataException($"Missing TSV column '{required}': {path}");

        var rows = new List<TsvRow>();
        int lineNumber = 1;
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            if (line.Length == 0) continue;
            string[] values = line.TrimEnd('\r').Split('\t');
            if (values.Length > headers.Length)
                throw new InvalidDataException($"Too many columns at {path}:{lineNumber}");
            rows.Add(new TsvRow(path, lineNumber, columns, values));
        }
        return new TsvTable(columns, rows);
    }

    public bool HasColumn(string name) => _columns.ContainsKey(name);
}

internal sealed class TsvRow
{
    private readonly string _path;
    private readonly int _line;
    private readonly IReadOnlyDictionary<string, int> _columns;
    private readonly string[] _values;

    public TsvRow(string path, int line, IReadOnlyDictionary<string, int> columns, string[] values)
    {
        _path = path;
        _line = line;
        _columns = columns;
        _values = values;
    }

    public string Get(string name)
    {
        if (!_columns.TryGetValue(name, out int index))
            throw new InvalidDataException($"Unknown TSV column '{name}' at {_path}:{_line}");
        return index < _values.Length ? _values[index] : string.Empty;
    }

    public int GetInt(string name)
    {
        string text = Get(name);
        return int.TryParse(text, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out int value)
            ? value
            : throw new InvalidDataException($"Invalid integer '{text}' in {name} at {_path}:{_line}");
    }
}
