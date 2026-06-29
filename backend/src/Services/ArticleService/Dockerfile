FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Services/ArticleService/ArticleService.csproj", "Services/ArticleService/"]
COPY ["src/Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Services/ArticleService/ArticleService.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/Services/ArticleService"
RUN dotnet build "ArticleService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArticleService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5001
EXPOSE 5002
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArticleService.dll"]