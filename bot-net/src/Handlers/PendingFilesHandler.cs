namespace Bot.Handlers;

public class PendingFilesHandler
{
    private readonly Dictionary<string, string> _files = new();

    public string RegisterFile(string file)
    {
        if (_files.TryGetValue(file, out var value)) return value;
        
        value = Guid.NewGuid().ToString();
        _files.Add(file, value);

        return value;
    }

    public string? GetFile(string guid)
    {
        return _files.GetValueOrDefault(guid);
    }
}