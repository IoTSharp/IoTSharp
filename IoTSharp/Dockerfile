#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 2927

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["IoTSharp/IoTSharp.csproj", "IoTSharp/"]
COPY ["IoTSharp.Extensions/IoTSharp.Extensions.csproj", "IoTSharp.Extensions/"]
COPY ["IoTSharp.Extensions.AspNetCore/IoTSharp.Extensions.AspNetCore.csproj", "IoTSharp.Extensions.AspNetCore/"]
RUN dotnet restore "IoTSharp/IoTSharp.csproj"
COPY . .
WORKDIR "/src/IoTSharp"
RUN dotnet build "IoTSharp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IoTSharp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IoTSharp.dll"]