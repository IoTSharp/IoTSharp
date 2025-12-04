#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get  -y update   && \
    apt-get install  -y --no-install-recommends apt-utils libgdiplus libc6-dev lsof net-tools wget sqlite3 iputils-ping inetutils-tools   curl libfontconfig1 && \
    apt-get autoclean 
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 1883 
EXPOSE 8883 

FROM 	mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN apt-get  -y update   && \
	apt-get install  -y  --no-install-recommends apt-utils libgdiplus libc6-dev lsof net-tools wget sqlite3 iputils-ping inetutils-tools  build-essential  libfontconfig1 curl   gnupg2 gnupg    && \
	apt-get autoclean  

RUN  apt-get update &&   apt-get install -y ca-certificates curl gnupg && \
	curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key |  gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg && \
	NODE_MAJOR=22  && \
	echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" |  tee /etc/apt/sources.list.d/nodesource.list && \
	apt-get update &&  apt-get install nodejs  -y && \
	ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime && \
	npm config set registry https://registry.npmmirror.com && \
	export NODE_OPTIONS=--openssl-legacy-provider && \
	apt-get autoclean 

WORKDIR /src
COPY ["ClientApp/package.json", "ClientApp/package.json"]
WORKDIR /src/ClientApp/
RUN  npm install 
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["IoTSharp/IoTSharp.csproj", "IoTSharp/"]
COPY ["IoTSharp.Contracts/IoTSharp.Contracts.csproj", "IoTSharp.Contracts/"]
COPY ["IoTSharp.Data.Storage/IoTSharp.Data.Cassandra/IoTSharp.Data.Cassandra.csproj", "IoTSharp.Data.Storage/IoTSharp.Data.Cassandra/"]
COPY ["IoTSharp.Data/IoTSharp.Data.csproj", "IoTSharp.Data/"]
COPY ["IoTSharp.Extensions/IoTSharp.Extensions.csproj", "IoTSharp.Extensions/"]
COPY ["IoTSharp.Data.Storage/IoTSharp.Data.ClickHouse/IoTSharp.Data.ClickHouse.csproj", "IoTSharp.Data.Storage/IoTSharp.Data.ClickHouse/"]
COPY ["IoTSharp.Data.Storage/IoTSharp.Data.InMemory/IoTSharp.Data.InMemory.csproj", "IoTSharp.Data.Storage/IoTSharp.Data.InMemory/"]
COPY ["IoTSharp.Data.Storage/IoTSharp.Data.MySQL/IoTSharp.Data.MySQL.csproj", "IoTSharp.Data.Storage/IoTSharp.Data.MySQL/"]
COPY ["IoTSharp.Data.Storage/IoTSharp.Data.Oracle/IoTSharp.Data.Oracle.csproj", "IoTSharp.Data.Storage/IoTSharp.Data.Oracle/"]
COPY ["IoTSharp.Data.Storage/IoTSharp.Data.PostgreSQL/IoTSharp.Data.PostgreSQL.csproj", "IoTSharp.Data.Storage/IoTSharp.Data.PostgreSQL/"]
COPY ["IoTSharp.Data.Storage/IoTSharp.Data.Sqlite/IoTSharp.Data.Sqlite.csproj", "IoTSharp.Data.Storage/IoTSharp.Data.Sqlite/"]
COPY ["IoTSharp.Data.Storage/IoTSharp.Data.SqlServer/IoTSharp.Data.SqlServer.csproj", "IoTSharp.Data.Storage/IoTSharp.Data.SqlServer/"]
COPY ["IoTSharp.Data.TimeSeries/IoTSharp.Data.TimeSeries.csproj", "IoTSharp.Data.TimeSeries/"]
COPY ["IoTSharp.Extensions.DependencyInjection/IoTSharp.Extensions.DependencyInjection.csproj", "IoTSharp.Extensions.DependencyInjection/"]
COPY ["IoTSharp.Extensions.EFCore/IoTSharp.Extensions.EFCore.csproj", "IoTSharp.Extensions.EFCore/"]
COPY ["IoTSharp.EventBus.CAP/IoTSharp.EventBus.CAP.csproj", "IoTSharp.EventBus.CAP/"]
COPY ["IoTSharp.EventBus/IoTSharp.EventBus.csproj", "IoTSharp.EventBus/"]
COPY ["IoTSharp.EventBus.Shashlik/IoTSharp.EventBus.Shashlik.csproj", "IoTSharp.EventBus.Shashlik/"]
COPY ["IoTSharp.Extensions.AspNetCore/IoTSharp.Extensions.AspNetCore.csproj", "IoTSharp.Extensions.AspNetCore/"]
COPY ["IoTSharp.Extensions.QuartzJobScheduler/IoTSharp.Extensions.QuartzJobScheduler.csproj", "IoTSharp.Extensions.QuartzJobScheduler/"]
COPY ["IoTSharp.Extensions.X509/IoTSharp.Extensions.X509.csproj", "IoTSharp.Extensions.X509/"]
COPY ["IoTSharp.Interpreter/IoTSharp.Interpreter.csproj", "IoTSharp.Interpreter/"]
COPY ["IoTSharp.TaskActions/IoTSharp.TaskActions.csproj", "IoTSharp.TaskActions/"]

ARG BUILD_CONFIGURATION=Release

RUN dotnet restore "IoTSharp/IoTSharp.csproj"
COPY . .
WORKDIR "/src/IoTSharp"
RUN dotnet build "./IoTSharp.csproj" --no-restore   -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "./IoTSharp.csproj" --no-restore   -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER app
WORKDIR /app
VOLUME ["/app/security/"]
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IoTSharp.dll"]