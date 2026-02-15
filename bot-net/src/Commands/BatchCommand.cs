using Bot.Handlers;
using Bot.Handlers;
using Telegram.Bot.Types;
using Message = WTelegram.Types.Message;

namespace Bot.Commands;

public class BatchCommand : ICommand
{
    private readonly BatchHandler _batchHandler;

    public BatchCommand(BatchHandler batchHandler)
    {
        _batchHandler = batchHandler;
    }

    public async Task Execute(string[] args, WTelegram.Types.Message msg)
    {
        await _batchHandler.HandleBatch();
    }

    public string Key => "/batch";
    public string Description => "Batches all media in watch folder.";
    public string Usage => "/batch";
}
