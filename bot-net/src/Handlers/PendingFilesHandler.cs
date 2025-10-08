namespace Bot.Handlers;

public class PendingFilesHandler
{
    private readonly Dictionary<string, string> _fileToGuid = new();
    private readonly Dictionary<string, string> _guidToFile = new();

    public string RegisterFile(string file)
    {
        if (_fileToGuid.TryGetValue(file, out var value))
            return value;

        value = Guid.NewGuid().ToString();
        _fileToGuid[file] = value;
        _guidToFile[value] = file;

        return value;
    }

    public string? GetFile(string guid)
    {
        return _guidToFile.GetValueOrDefault(guid);
    }

}