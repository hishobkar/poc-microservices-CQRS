FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/QueryService/QueryService.csproj", "Services/QueryService/"]
COPY ["src/Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Services/QueryService/QueryService.csproj"

COPY src/ .
WORKDIR "/src/Services/QueryService"
RUN dotnet build "QueryService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QueryService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5003
EXPOSE 5004
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QueryService.dll"]