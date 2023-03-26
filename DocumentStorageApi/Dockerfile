FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DocumentStorageApi/DocumentStorageApi.csproj", "DocumentStorageApi/"]
RUN dotnet restore "DocumentStorageApi/DocumentStorageApi.csproj"
COPY . .
WORKDIR "/src/DocumentStorageApi"
RUN dotnet build "DocumentStorageApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DocumentStorageApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./DocumentStorageApi/appsettings.json /app/
ENTRYPOINT ["dotnet", "DocumentStorageApi.dll"]
