FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["catApplication/catApplication.csproj", "catApplication/"]
RUN dotnet restore "catApplication/catApplication.csproj"
COPY . .
WORKDIR "/src/catApplication"
RUN dotnet build "catApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "catApplication.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "catApplication.dll"]
