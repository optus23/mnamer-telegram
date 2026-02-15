using Bot;
using Bot.Utils;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace Bot.Handlers;

public class BatchHandler
{
    private readonly WTelegram.Bot _bot;
    private readonly DirectoryHandler _directoryHandler;
    private readonly MnamerHandler _mnamer;
    private readonly PendingFilesHandler _pendingFilesHandler;
    private readonly int _chatId;
    
    // MessageId -> BatchGroupGuid
    private readonly ConcurrentDictionary<int, string> _promptToBatchGroup = new();
    
    // BatchGroupGuid -> BatchGroup
    private readonly ConcurrentDictionary<string, BatchGroup> _batchGroups = new();

    public BatchHandler(WTelegram.Bot bot, DirectoryHandler directoryHandler, MnamerHandler mnamer, PendingFilesHandler pendingFilesHandler, int chatId)
    {
        _bot = bot;
        _directoryHandler = directoryHandler;
        _mnamer = mnamer;
        _pendingFilesHandler = pendingFilesHandler;
        _chatId = chatId;
    }

    public async Task HandleBatch()
    {
        await _bot.SendMessage(_chatId, "Scanning for files...");
        
        var files = Directory.GetFiles(_directoryHandler.WatchDirectory, "*.*", SearchOption.AllDirectories)
            .Where(f => NewFileHandler.VideoExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (files.Count == 0)
        {
            await _bot.SendMessage(_chatId, "No files found in watch directory.");
            return;
        }

        await ProcessFiles(files);
    }

    private async Task ProcessFiles(List<string> files)
    {
        var groups = new Dictionary<string, BatchGroup>(); // Key = Destination/Title

        foreach (var file in files)
        {
            var (destination, mediaType, detectedId) = await GetMnamerDestination(file);
            
            if (destination == null)
            {
                Log.Info($"Could not identify file: {file}");
                continue;
            }

            if (!groups.TryGetValue(destination, out var group))
            {
                group = new BatchGroup { Title = destination, Destination = destination, DetectedId = detectedId, ForcedType = mediaType };
                groups[destination] = group;
                _batchGroups[group.Id] = group;
            }
            
            group.Files.Add(file);
        }

        if (groups.Count == 0)
        {
            await _bot.SendMessage(_chatId, "Could not identify any files.");
            return;
        }

        await _bot.SendMessage(_chatId, $"Found {groups.Count} groups. Sending details...");

        foreach (var group in groups.Values)
        {
            await SendGroupMessage(group);
        }

        await _bot.SendMessage(_chatId, "✅ **Batch scan complete.**\nYou can accept all remaining groups or handle them individually.", 
            ParseMode.Markdown, 
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Accept All Remaining", "batch:accept_all:global")
            }));
    }

    private async Task<(string? Destination, MediaType? Type, string? Id)> GetMnamerDestination(string file, string? forcedId = null, MediaType? forcedType = null)
    {
        const string movieFormat = "MOVIE__SEP__{id_tmdb}__SEP__{name} ({year})";
        const string showFormat = "SHOW__SEP__{id_tvdb}__SEP__{series} S{season:00}";
        
        var arguments =
            $"--test --batch --no-style --language {_mnamer.Language} --movie-format \"{movieFormat}\" --episode-format \"{showFormat}\" --movie-api tmdb --episode-api tvdb \"{file}\"";

        if (forcedId != null && forcedType != null)
        {
            if (forcedType == MediaType.Movie)
                arguments += $" --id-tmdb {forcedId}";
            else
                arguments += $" --id-tvdb {forcedId}";
        }

        var output = await _mnamer.ExecuteMnamer(arguments);
        
        var match = Regex.Match(output, "moving to .+/(.+)$", RegexOptions.Multiline);
        if (!match.Success) return (null, null, null);

        var lastPart = match.Groups[1].Value;
        var parts = lastPart.Split(new[] { "__SEP__" }, StringSplitOptions.None);
        
        if (lastPart.StartsWith("MOVIE__SEP__") && parts.Length >= 3)
        {
            // MOVIE, ID, Destination
            return (parts[2], MediaType.Movie, parts[1]);
        }
        else if (lastPart.StartsWith("SHOW__SEP__") && parts.Length >= 3)
        {
             // SHOW, ID, Destination
             return (parts[2], MediaType.Episode, parts[1]);
        }
        
        return (null, null, null);
    }

    private async Task SendGroupMessage(BatchGroup group)
    {
        var msg = FormulateMessage(group);
        
        var markup = new InlineKeyboardMarkup(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData("Move All", $"batch:move:{group.Id}"),
                InlineKeyboardButton.WithCallbackData("Correct ID", $"batch:edit:{group.Id}"),
            },
            new []
            {
                 InlineKeyboardButton.WithCallbackData("Ignore", $"batch:ignore:{group.Id}")
            }
        });



        var sent = await _bot.SendMessage(_chatId, msg, ParseMode.Markdown, replyMarkup: markup);
        group.MessageId = sent.Id;
    }

    private string FormulateMessage(BatchGroup group)
    {
        var link = "";
        var id = group.ForcedId ?? group.DetectedId;
        var type = group.ForcedType;
        
        if (!string.IsNullOrEmpty(id))
        {
             if (type == MediaType.Movie)
                 link = $" [TMDB](https://www.themoviedb.org/movie/{id})";
             else if (type == MediaType.Episode)
                 link = $" [TVDB](https://www.thetvdb.com/search?query={id})";
        }

        return $"📦 **Batch Group**: `{group.Title}`{link}\n" +
               $"Files ({group.Files.Count}):\n" +
               string.Join("\n", group.Files.OrderBy(f => f).Select(f => $"- `{Path.GetFileName(f)}`")) + 
               (group.ForcedId != null ? $"\n\n(Forced ID: {group.ForcedId})" : "");
    }
    
    public async Task HandleCallback(string action, string guid, Message message)
    {
        if (action == "accept_all")
        {
            await ExecuteAcceptAll(message);
            return;
        }

        if (!_batchGroups.TryGetValue(guid, out var group))
        {
             await _bot.EditMessageText(message.Chat.Id, message.Id, "Batch group expired or not found.");
             return;
        }

        if (action == "move")
        {
            await ExecuteMove(group, message.Id);
        }
        else if (action == "edit")
        {
            var prompt = await _bot.SendMessage(_chatId, $"Please reply to this message with the ID for **{group.Title}**.\n(Prefix with `tmdb`/`movie` or `tvdb`/`show` if needed)", 
                replyMarkup: new ForceReplyMarkup());
            _promptToBatchGroup[prompt.Id] = group.Id;
        }
        else if (action == "ignore")
        {
            await _bot.EditMessageText(message.Chat.Id, message.Id, "Skipped.");
            _batchGroups.TryRemove(guid, out _);
        }
    }

    public async Task HandleReply(int replyToMsgId, string text)
    {
        if (!_promptToBatchGroup.TryRemove(replyToMsgId, out var groupId)) return;
        if (!_batchGroups.TryGetValue(groupId, out var group)) return;

        // Parse ID like in NewFileHandler
        string? forcedId = null;
        MediaType? forcedType = null;
        
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 1)
        {
             var prefix = parts[0].ToLowerInvariant();
             if (prefix == "tmdb" || prefix == "movie") { forcedType = MediaType.Movie; forcedId = parts[1]; }
             else if (prefix == "tvdb" || prefix == "show" || prefix == "series") { forcedType = MediaType.Episode; forcedId = parts[1]; }
        }
        
        if (forcedId == null)
        {
             forcedId = text.Trim();
        }
        
        group.ForcedId = forcedId;
        group.ForcedType = forcedType;
        
        await _bot.SendMessage(_chatId, $"Re-evaluating group with ID {forcedId}...");
        
        if (group.Files.Count > 0)
        {
            var (newDest, _, _) = await GetMnamerDestination(group.Files[0], forcedId, forcedType);
            if (newDest != null) group.Title = newDest;
        }
        
        await SendGroupMessage(group);
    }

    private async Task ExecuteAcceptAll(Message summaryMessage)
    {
        var groups = _batchGroups.Values.ToList();
        await _bot.EditMessageText(_chatId, summaryMessage.Id, $"Processing {groups.Count} groups...");

        foreach (var group in groups)
        {
            if (group.MessageId != 0)
            {
                await ExecuteMove(group, group.MessageId);
            }
        }
        
        await _bot.EditMessageText(_chatId, summaryMessage.Id, "✅ All remaining groups processed.");
    }

    private async Task ExecuteMove(BatchGroup group, int messageId)
    {
        // Check if group is still valid (might have been removed concurrently?)
        if (!_batchGroups.ContainsKey(group.Id)) return;

        await _bot.EditMessageText(_chatId, messageId, $"Moving {group.Files.Count} files to `{group.Title}`...");
        
        foreach (var file in group.Files)
        {
             var arguments =
            $"--batch --no-style --language {_mnamer.Language} --movie-directory \"{_mnamer.MovieDirectoryFormat}\" --movie-format \"{_mnamer.MovieFormat}\" --episode-directory \"{_mnamer.EpisodeDirectoryFormat}\" --episode-format \"{_mnamer.EpisodeFormat}\" --movie-api tmdb --episode-api tvdb \"{file}\"";

            if (group.ForcedId != null)
            {
                if (group.ForcedType == MediaType.Movie) arguments += $" --id-tmdb {group.ForcedId}";
                else if (group.ForcedType == MediaType.Episode) arguments += $" --id-tvdb {group.ForcedId}";
                else arguments += $" --id-tvdb {group.ForcedId}"; // Default to TVDB?
            }

            await _mnamer.ExecuteMnamer(arguments);
        }
        
        await _bot.EditMessageText(_chatId, messageId, $"✅ batch moved to `{group.Title}`.");
        _batchGroups.TryRemove(group.Id, out _);
    }
}

public class BatchGroup 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; }
    public string Destination {get; set;}
    public List<string> Files { get; set; } = new();
    public string? ForcedId { get; set; }
    public MediaType? ForcedType { get; set; }
    public string? DetectedId { get; set; }
    public int MessageId { get; set; }
}
