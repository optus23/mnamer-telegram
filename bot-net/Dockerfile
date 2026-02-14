FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./src/*.csproj ./
RUN dotnet restore
COPY ./src/ ./
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

ARG PUID=1000
ARG GUID=1000

ENV PIPX_HOME=/opt/pipx
ENV PIPX_BIN_DIR=/usr/local/bin

RUN apt-get update && apt-get install -y python3 python3-pip pipx \
    && pipx install mnamer \
    && rm -rf /var/lib/apt/lists/*

RUN groupadd -g ${GUID} botgroup || true && \
    useradd -l -u ${PUID} -g ${GUID} -m botuser || true

WORKDIR /app
COPY --from=build /app ./

RUN chown -R ${PUID}:${GUID} /app

USER botuser

ENTRYPOINT ["dotnet", "Bot.dll"]