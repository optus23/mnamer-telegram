using System.Text;

namespace Bot.Handlers;

public class DirectoryHandler
{
    public string MoviesDirectory { get; }
    public string ShowsDirectory { get; }
    public string WatchDirectory { get; }

    public DirectoryHandler()
    {
        MoviesDirectory = Environment.GetEnvironmentVariable("MOVIES_DIR") ?? "/data/movies";
        ShowsDirectory = Environment.GetEnvironmentVariable("SHOWS_DIR") ?? "/data/shows";
        WatchDirectory  = Environment.GetEnvironmentVariable("WATCH_DIR")  ?? "/data/watch";
    }
    
    public string CheckConfiguration()
    {
        var sb = new StringBuilder();
        sb.AppendLine("📂 *Directory Configuration Check:*");

        CheckPath(sb, "Movies", MoviesDirectory);
        CheckPath(sb, "Shows", ShowsDirectory);
        CheckPath(sb, "Watch", WatchDirectory);

        return sb.ToString();
    }

    private void CheckPath(StringBuilder sb, string label, string path)
    {
        if (!Directory.Exists(path))
        {
            sb.AppendLine($"❌ {label}: `{path}` *(does not exist or is not accessible)*");
            return;
        }

        try
        {
            // Try writing a small test file to confirm write permissions
            var testFile = Path.Combine(path, ".write_test");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);

            // Gather a quick summary of contents
            var entries = Directory.GetFileSystemEntries(path);
            var fileCount = Directory.GetFiles(path).Length;
            var dirCount = Directory.GetDirectories(path).Length;

            sb.AppendLine($"✅ {label}: `{path}` *(accessible and writable)*");
            sb.AppendLine($" ├─ Contains: {dirCount} folders, {fileCount} files");

            // Show a short preview of contents
            if (entries.Length > 0)
            {
                sb.AppendLine(" ├─ Example contents:");
                foreach (var entry in entries.Take(5)) // limit to first 5 entries
                {
                    var name = Path.GetFileName(entry);
                    sb.AppendLine($" │  • `{name}`");
                }

                if (entries.Length > 5)
                    sb.AppendLine($" │  ...and {entries.Length - 5} more");
            }
            else
            {
                sb.AppendLine(" ├─ (empty directory)");
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"❌ {label}: `{path}` *(not writable — `{ex.Message}`)*");
        }
    }

}