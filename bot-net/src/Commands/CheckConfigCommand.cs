using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Bot.CallbackQueries.Callbacks;
using Bot.Handlers;
using Bot.Utils;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Message = WTelegram.Types.Message;

namespace Bot.Commands;

public class CheckConfigCommand : ICommand
{
    private readonly WTelegram.Bot _bot;
    private readonly MnamerHandler _mnamer;
    private readonly DirectoryHandler _directoryHandler;

    public CheckConfigCommand(WTelegram.Bot bot, DirectoryHandler directoryHandler, MnamerHandler mnamerHandler)
    {
        _bot = bot;
        _directoryHandler = directoryHandler;
        _mnamer = mnamerHandler;
    }

    public async Task Execute(string[] args, Message message)
    {
        var report = $"{_directoryHandler.CheckConfiguration()}\n\n{_mnamer.GetConfigurationSummary()}";

        await _bot.SendMessage(
            chatId: message.Chat.Id,
            text: report,
            parseMode: ParseMode.Markdown
        );
    }

    public string Key => "/checkconfig";
    public string Description => "Checks the configuration of the bot.";
    public string Usage => "/checkconfig";

}