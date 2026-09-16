FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln .
COPY lib/hf-state-streaming/engine-dotnet/src/Engine.Core/Engine.Core.csproj lib/hf-state-streaming/engine-dotnet/src/Engine.Core/
COPY src/Game.Shared/Game.Shared.csproj src/Game.Shared/
COPY src/Game.Server/Game.Server.csproj src/Game.Server/
COPY src/Game.Client/Game.Client.csproj src/Game.Client/
COPY tests/Game.Shared.Tests/Game.Shared.Tests.csproj tests/Game.Shared.Tests/
COPY tests/Game.Server.Tests/Game.Server.Tests.csproj tests/Game.Server.Tests/
COPY tests/Game.Client.Tests/Game.Client.Tests.csproj tests/Game.Client.Tests/

RUN dotnet restore src/Game.Server/Game.Server.csproj

# Copy all files and build
COPY . .
WORKDIR /app/src/Game.Server
RUN dotnet publish -c Release -o /app/out /p:PublishAot=true

# Use alpine for minimal footprint for native AOT binary
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 9050/udp
ENTRYPOINT ["./Game.Server"]
