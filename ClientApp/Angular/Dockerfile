#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
RUN curl -fsSL https://deb.nodesource.com/setup_14.x | bash -  &&  apt-get install -y nodejs  && apt-get install -y build-essential
WORKDIR /src
COPY ["ClientApp/Angular/IoTSharp.ClientApp.Angular.csproj", "ClientApp/Angular/"]
RUN dotnet restore "ClientApp/Angular/IoTSharp.ClientApp.Angular.csproj"
COPY . .
WORKDIR "/src/ClientApp/Angular"
RUN dotnet build "IoTSharp.ClientApp.Angular.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IoTSharp.ClientApp.Angular.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IoTSharp.ClientApp.Angular.dll"]