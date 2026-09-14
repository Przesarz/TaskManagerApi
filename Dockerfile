FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY TaskManager.Api/TaskManager.Api.csproj TaskManager.Api/

RUN dotnet restore TaskManager.Api/TaskManager.Api.csproj

COPY . .

RUN dotnet build TaskManager.Api/TaskManager.Api.csproj -c Release --no-restore

RUN dotnet publish TaskManager.Api/TaskManager.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]