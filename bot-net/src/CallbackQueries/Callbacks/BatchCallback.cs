using Bot.Handlers;
using Telegram.Bot.Types;

namespace Bot.CallbackQueries.Callbacks;

[Callback(Id)]
public class BatchCallback : ICallbackQuery
{
    public const string Id = "batch";
    private readonly BatchHandler _batchHandler;
    private readonly string _action;
    private readonly string _guid;

    public BatchCallback(BatchHandler batchHandler, string action, string guid)
    {
        _batchHandler = batchHandler;
        _action = action;
        _guid = guid;
    }

    public async Task ExecuteAsync(Message? message)
    {
        if (message == null) return;
        await _batchHandler.HandleCallback(_action, _guid, message);
    }

    public static ICallbackQuery Create(string[] fields, BotDispatcher dispatcher)
    {
        // format: batch:action:guid
        return new BatchCallback(dispatcher.BatchHandler, fields[0], fields[1]);
    }
}
