#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
MAINTAINER Yanhong Ma 2022 <mysticboy@live.com>
RUN  apt-get  -y update   && \
     apt-get install  -y apt-utils libgdiplus libc6-dev lsof net-tools wget sqlite3 iputils-ping inetutils-tools   curl libfontconfig1 && \
     apt-get autoclean 
WORKDIR /app
EXPOSE 80  
EXPOSE 1883 
EXPOSE 8883 


FROM 	mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
RUN  	apt-get  -y update   && \
	apt-get install  -y  apt-utils libgdiplus libc6-dev lsof net-tools wget sqlite3 iputils-ping inetutils-tools    libfontconfig1 curl   gnupg2 gnupg    && \
	apt-get autoclean  
RUN KEYRING=/usr/share/keyrings/nodesource.gpg && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource.gpg.key | gpg --dearmor |  tee "$KEYRING" >/dev/null && \
	gpg --no-default-keyring --keyring "$KEYRING" --list-keys && \
	VERSION=node_16.x &&  DISTRO=bullseye  && \
	echo "deb [signed-by=$KEYRING] https://deb.nodesource.com/$VERSION $DISTRO main" |  tee /etc/apt/sources.list.d/nodesource.list && \
	apt-get  -y   -q update  && \
    apt-get install -y nodejs && \
	ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime && \
	npm config set registry https://registry.npmmirror.com && \
	export NODE_OPTIONS=--openssl-legacy-provider && \
	apt-get autoclean 

    
WORKDIR /src
COPY ["IoTSharp/ClientApp/package.json", "IoTSharp/ClientApp/package.json"]
RUN   npm install --prefix ./IoTSharp/ClientApp/
COPY ["IoTSharp/IoTSharp.csproj", "IoTSharp/"]
COPY ["IoTSharp.Data/IoTSharp.Data.csproj", "IoTSharp.Data/"]
COPY ["IoTSharp.Interpreter/IoTSharp.Interpreter.csproj", "IoTSharp.Interpreter/"]
COPY ["IoTSharp.TaskAction/IoTSharp.TaskAction.csproj", "IoTSharp.TaskAction/"]
COPY ["IoTSharp.Data.SqlServer/IoTSharp.Data.SqlServer.csproj", "IoTSharp.Data.SqlServer/"]
COPY ["IoTSharp.Data.InMemory/IoTSharp.Data.InMemory.csproj", "IoTSharp.Data.InMemory/"]
COPY ["IoTSharp.Data.Sqlite/IoTSharp.Data.Sqlite.csproj", "IoTSharp.Data.Sqlite/"]
COPY ["IoTSharp.Data.Oracle/IoTSharp.Data.Oracle.csproj", "IoTSharp.Data.Oracle/"]
COPY ["IoTSharp.Data.PostgreSQL/IoTSharp.Data.PostgreSQL.csproj", "IoTSharp.Data.PostgreSQL/"]
COPY ["IoTSharp.Data.MySQL/IoTSharp.Data.MySQL.csproj", "IoTSharp.Data.MySQL/"]
COPY ["IoTSharp.Data.Cassandra/IoTSharp.Data.Cassandra.csproj", "IoTSharp.Data.Cassandra/"]
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
