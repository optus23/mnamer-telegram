using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Message = WTelegram.Types.Message;

namespace Bot.Commands;

public class StartCommand : ICommand
{
    private readonly WTelegram.Bot _bot;

    public StartCommand(WTelegram.Bot bot)
    {
        _bot = bot;
    }

    public async Task Execute(string[] args, Message msg)
    {
        const string message = @"👋 Hello! I'm your *Media Renamer Bot*.
I am currently watching for new media files. 🎬📺
    
⚠️ *Before using any move or rename commands*, please make sure your directories are configured correctly.
👉 Use the command /checkconfig to verify your setup.

Use `/help` for a list of available commands.

—
💻 *Developed by* [christt105](https://github.com/christt105)
⚙️ Powered by [mnamer](https://github.com/jkwill87/mnamer), [TMDb](https://www.themoviedb.org), and [TheTVDB](https://thetvdb.com)";
        await _bot.SendMessage(msg.Chat.Id, message, ParseMode.MarkdownV2,
            linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true });
    }

    public string Key => "/start";
    public string Description => "Welcome message";
    public string Usage => "/start";
}