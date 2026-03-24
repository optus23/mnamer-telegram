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

I will be scanning the watch folder for new media files. You can also use the /search command to manually trigger a scan. When I find one, I'll try to identify it and send you a message with the details. If I can't identify it, I'll let you know and you can help me out by providing the correct information. If I am wrong, you can also correct me by replying to the message with the correct ID using [TMDb](https://www.themoviedb.org) for movies and [TheTVDB](https://thetvdb.com) for TV shows.

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