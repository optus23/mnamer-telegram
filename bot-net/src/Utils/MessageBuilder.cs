
namespace Bot.Utils;

public static class MessageBuilder
{
    public static string FormatTmdbImageUrl(string tmdbImageUrl)
    {
        return $"https://image.tmdb.org/t/p/w500{tmdbImageUrl}";
    }
}