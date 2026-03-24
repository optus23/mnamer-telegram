# mnamer-telegram

A Telegram bot to automate the organization of your media library using the powerful [mnamer](https://github.com/jkwill87/mnamer) tool.

![Screenshot Start](./docs/HelpAndStartCommand.png)

> [!WARNING]  
> **Disclaimer:** This tool handles file movements and renaming. I am **not responsible for any data loss** or files being moved to incorrect locations. It is highly recommended to perform a test with dummy files or a backup before running it on your primary library.

> [!IMPORTANT]  
> **Configuration Complexity:** Setting up Docker volumes and environment variables correctly can be complex depending on your OS and permissions. If you encounter any issues or have questions, please **open an issue** in this repository.

## Features

- **📂 Watch Folder Monitoring**: Automatically detects new video files in your configured download directory.
- **🤖 Automatic Organization**: Uses `mnamer` to rename and move files to your library (Movies/TV Shows).
- **💬 Interactive Telegram Interface**: 
    - Receive notifications when files are processed.
    - **Approve Moves**: One-click confirmation to move files.
    - **Manual Correction**: Reply with `tmdb <id>` or `tvdb <id>` if the detection is wrong.

![Screenshot Scan](./docs/ExampleRename.png)

## Setup & Deployment

The recommended way to run this bot is via **Docker Compose**.

### Prerequisites

- **Docker** and **Docker Compose** installed.
- A **Telegram Bot Token** (create your bot with [@BotFather](https://t.me/BotFather)).
- Your **Telegram User ID** (from [@username_to_id_bot](https://t.me/username_to_id_bot)).

### Obtaining Telegram API Credentials

To get your `TELEGRAM_API_ID` and `TELEGRAM_API_HASH`:

1.  Log in to your Telegram account at [my.telegram.org](https://my.telegram.org).
2.  Go to **API development tools**.
3.  Fill out the form (you can use any dummy data for URL/Description).
4.  Click **Create application**.
5.  Copy the `App api_id` and `App api_hash` values.

*For more details, see the [official guide](https://core.telegram.org/api/obtaining_api_id).*

### Configuration

1.  **Clone the repository**.
2.  **Create a `.env` file** in the root directory (you can copy `.env.example` as a starting point):
    ```env
    TELEGRAM_API_ID=your_api_id
    TELEGRAM_API_HASH=your_api_hash
    TELEGRAM_BOT_TOKEN=your_bot_token
    TELEGRAM_AUTH_USER_ID=your_user_id
    
    # Paths inside the container (Verify these match your volume mappings)
    # Recommended: Use a single volume for atomic moves (instant & no copy)
    WATCH_DIR=/media/downloads
    MOVIES_DIR=/media/movies
    SHOWS_DIR=/media/shows
    ```
3.  **Review `docker-compose.yml`** (an example file is included in the repository):
    
    > **Performance Tip**: Map a single volume (e.g., `/mnt/data:/media`) containing both your downloads and library folders. This allows the bot to "move" files instantly (atomic move) instead of copying them between drives.

    ```yaml
    services:
      bot-mnamer:
        build:
          context: ./bot-net
          dockerfile: Dockerfile
        restart: unless-stopped
        env_file: .env
        volumes:
          - ./appdata:/data            # Database location
          - /mnt/data:/media           # Single volume for all media
    ```
    *In this example, your host `/mnt/data` should contain `downloads`, `movies`, and `shows` folders.*

### Running

Start the bot container:

```bash
docker-compose up -d
```

View logs:

```bash
docker-compose logs -f
```

## ⚠️ Known Limitations & Docker Compatibility

The real-time folder monitoring (`FileSystemWatcher`) is primarily designed and tested for **Linux hosts**.

If you are running Docker on **Windows or macOS**, file system events from the host might not propagate to the container due to how Docker handles volume mounts (gRPC FUSE/VirtioFS). In these environments, the bot may not detect new files automatically.
- **Workaround**: Use the `/search` command to trigger a manual scan of the watch folder.
- **Technical Details**: Read more about [File System Watch issues on Docker mounts](https://forums.docker.com/t/file-system-watch-does-not-work-with-mounted-volumes/12038/18).

## Usage

1.  **Automatic**: The bot watches `/downloads` (mapped path). When a video file is detected, it will message you on Telegram.
2.  **Scan**: Send `/search` to the bot to scan the watch folder for existing files.
3.  **Corrections**: Reply to any message with `tmdb <id>` or `tvdb <id>` to force a match.

## Contributions

Refactors, features, and pull requests are welcome!

### Support the Project

If you find this bot useful, please consider starring the repository! ⭐

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/christt105)

## Attributions

This project heavily relies on and gratefully acknowledges:

- **[mnamer](https://github.com/jkwill87/mnamer)**: The core media organization tool.
- **[TheMovieDB (TMDB)](https://www.themoviedb.org/)**: For movie metadata.
- **[TheTVDB](https://thetvdb.com/)**: For TV show metadata.
