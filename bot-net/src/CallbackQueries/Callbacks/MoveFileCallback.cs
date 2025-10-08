using Bot.Handlers;
using Telegram.Bot.Types;

namespace Bot.CallbackQueries.Callbacks;

[Callback(Id)]
public class MoveFileCallback : ICallbackQuery
{
    public const string Id = "move";
    private readonly WTelegram.Bot _bot;
    private readonly DirectoryHandler _directoryHandler;
    private readonly string _guid;
    private readonly MnamerHandler _mnamer;
    private readonly PendingFilesHandler _pendingFilesHandler;

    private MoveFileCallback(string guid,
        PendingFilesHandler pendingFilesHandler,
        MnamerHandler mnamerHandler,
        WTelegram.Bot bot,
        DirectoryHandler directoryHandler)
    {
        _guid = guid;
        _pendingFilesHandler = pendingFilesHandler;
        _mnamer = mnamerHandler;
        _bot = bot;
        _directoryHandler = directoryHandler;
    }


    public async Task ExecuteAsync(Message? message)
    {
        var file = _pendingFilesHandler.GetFile(_guid);

        if (file == null)
        {
            Log.Error($"File with guid {_guid} not found.");
            //TODO: if not found, try to get the file from inside the message text
            return;
        }

        var arguments =
            $"--test --batch --no-style --language {_mnamer.Language} --movie-directory \"{_mnamer.MovieDirectoryFormat}\" --movie-format \"{_mnamer.MovieFormat}\" --episode-directory \"{_mnamer.EpisodeDirectoryFormat}\" --episode-format \"{_mnamer.EpisodeFormat}\" --episode-api tvdb \"{_directoryHandler.WatchDirectory}/{file}\"";

        var result = await _mnamer.ExecuteMnamer(arguments);

        Log.Info(result);

        await _bot.EditMessageText(message.Chat.Id, message.Id, $"FILE: {file}\n\n{result}");
    }

    public static ICallbackQuery Create(string[] fields, BotDispatcher dispatcher)
    {
        var guid = fields[0];
        return new MoveFileCallback(guid, dispatcher.PendingFilesHandler, dispatcher.MnamerHandler, dispatcher.Bot,
            dispatcher.DirectoryHandler);
    }

    public static string Pack(string guid)
    {
        return CallbackDataPacker.Pack(Id, [guid]);
    }
}