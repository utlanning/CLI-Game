namespace Isekai.VSlice.Core.Content;

public sealed class ContentException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ContentException(IEnumerable<string> errors)
        : base(BuildMessage(errors))
    {
        Errors = errors.ToList();
    }

    private static string BuildMessage(IEnumerable<string> errors)
    {
        var list = errors.ToList();
        return "Content validation failed:\n- " + string.Join("\n- ", list);
    }
}