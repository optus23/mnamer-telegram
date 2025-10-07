FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./src/*.csproj ./
RUN dotnet restore

COPY ./src/ ./
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

RUN apt-get update && apt-get install -y python3 python3-pip pipx \
    && pipx ensurepath \
    && pipx install mnamer \
    && ln -s /root/.local/pipx/venvs/mnamer/bin/mnamer /usr/local/bin/mnamer \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Bot.dll"]
