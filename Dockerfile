#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Cesxhin.AnimeManga.DownloadService/", "./Cesxhin.AnimeManga.DownloadService/"]
COPY ["src/references/Cesxhin.AnimeManga.Modules/", "./references/Cesxhin.AnimeManga.Modules/"]
COPY ["src/references/Cesxhin.AnimeManga.Domain/", "./references/Cesxhin.AnimeManga.Domain/"]

RUN dotnet restore "./Cesxhin.AnimeManga.DownloadService/Cesxhin.AnimeManga.DownloadService.csproj"
COPY . .
WORKDIR "./Cesxhin.AnimeManga.DownloadService"

RUN dotnet build "Cesxhin.AnimeManga.DownloadService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cesxhin.AnimeManga.DownloadService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cesxhin.AnimeManga.DownloadService.dll"]