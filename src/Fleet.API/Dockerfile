#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Fleet.API/Fleet.API.csproj", "src/Fleet.API/"]
COPY ["src/Fleet.Core/Fleet.Core.csproj", "src/Fleet.Core/"]
RUN dotnet restore "src/Fleet.API/Fleet.API.csproj"
COPY . .
WORKDIR "/src/src/Fleet.API"
RUN dotnet build "Fleet.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fleet.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fleet.API.dll"]
