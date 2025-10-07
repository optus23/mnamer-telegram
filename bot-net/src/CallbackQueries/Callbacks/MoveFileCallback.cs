using Bot.Handlers;
using Telegram.Bot.Types;

namespace Bot.CallbackQueries.Callbacks;

[Callback(Id)]
public class MoveFileCallback : ICallbackQuery
{
    public const string Id = "move";
    private readonly string _guid;
    private readonly PendingFilesHandler _pendingFilesHandler;
    private readonly MnamerHandler _mnamer;

    private MoveFileCallback(string guid, PendingFilesHandler pendingFilesHandler, MnamerHandler mnamerHandler)
    {
        _guid = guid;
        _pendingFilesHandler = pendingFilesHandler;
        _mnamer = mnamerHandler;
    }


    public Task ExecuteAsync(Message? message)
    {
        _mnamer.ExecuteMnamer()
    }

    public static ICallbackQuery Create(string[] fields, BotDispatcher dispatcher)
    {
        var guid = fields[0];
        return new MoveFileCallback(guid, dispatcher.PendingFilesHandler, dispatcher.MnamerHandler);
    }

    public static string Pack(string guid)
    {
        return CallbackDataPacker.Pack(Id, [guid]);
    }
}