#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80  
EXPOSE 1883 
EXPOSE 8883 
EXPOSE 502 
EXPOSE 5683
EXPOSE 5684 
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["IoTSharp/IoTSharp.csproj", "IoTSharp/"]
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
