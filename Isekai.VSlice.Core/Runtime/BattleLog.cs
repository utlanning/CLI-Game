namespace Isekai.VSlice.Core.Runtime;

public sealed class BattleLog
{
    private readonly List<string> _lines = new();
    public IReadOnlyList<string> Lines => _lines;

    public void Add(string line) => _lines.Add(line);

    public void AddHeader(string title)
    {
        _lines.Add("");
        _lines.Add($"\x1b[1;97m=== {title} ===\x1b[0m");
    }
}
