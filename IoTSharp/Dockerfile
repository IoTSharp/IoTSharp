#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
MAINTAINER Yanhong Ma 2020
RUN echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian/ buster main contrib non-free" > /etc/apt/sources.list && \
	echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian/ buster-updates main contrib non-free" >> /etc/apt/sources.list && \
	echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian/ buster-backports main contrib non-free" >> /etc/apt/sources.list && \
	echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian-security buster/updates main contrib non-free" >> /etc/apt/sources.list && \
	echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian-security jessie/updates main contrib non-free" >> /etc/apt/sources.list && \
    apt-get  -y   -q update   && apt-get install  -y   -q  apt-utils libgdiplus libc6-dev lsof net-tools wget sqlite3 python3  bashtop   iputils-ping inetutils-tools  libssl-dev  libssl1.0.0 curl  && \
	apt-get autoremove -y &&  apt-get clean  &&  apt-get autoclean && rm  /var/cache/apt/* -rf &&  \
	ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime
#RUN curl -o TDengine-client.tar.gz "https://www.taosdata.com/download/download-gettingStarted.php?pkg=tdengine_linux&pkgName=TDengine-client-2.0.12.0-Linux-x64.tar.gz" && \
   #tar -xzvf TDengine-client.tar.gz && rm TDengine-client.tar.gz -f  && cd  $(ls TDengine-client*  -d) && ./install_client.sh && \
    #rm $(pwd) -rf
RUN echo "fs.file-max=6553500" >> /etc/sysctl.conf && \
    echo "* soft nofile 65535" >> /etc/security/limits.conf && \
    echo "* hard nofile 65535" >> /etc/security/limits.conf && \
    echo "session    required   pam_limits.so" >> /etc/security/limits.conf
WORKDIR /app
EXPOSE 80  
EXPOSE 1883 
EXPOSE 8883 
EXPOSE 502 
EXPOSE 5683
EXPOSE 5684 
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build

RUN echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian/ buster main contrib non-free" > /etc/apt/sources.list && \
	echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian/ buster-updates main contrib non-free" >> /etc/apt/sources.list && \
	echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian/ buster-backports main contrib non-free" >> /etc/apt/sources.list && \
	echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian-security buster/updates main contrib non-free" >> /etc/apt/sources.list && \
    apt-get  -y   -q update   && apt-get install  -y   -q  git  libssl-dev  curl gnupg libcurl4  && \
	ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime 
 

RUN curl -fsSL https://deb.nodesource.com/setup_lts.x | bash - && \
    apt-get install  -y   -q  nodejs && \
	apt remove cmdtest  && \
	apt remove yarn && \
	curl -sL https://dl.yarnpkg.com/debian/pubkey.gpg | gpg --dearmor |  tee /usr/share/keyrings/yarnkey.gpg >/dev/null && \
	echo "deb [signed-by=/usr/share/keyrings/yarnkey.gpg] https://dl.yarnpkg.com/debian stable main" |  tee /etc/apt/sources.list.d/yarn.list  && \
	apt-get install  -y   -q  yarn 

RUN    apt-get autoremove -y &&  apt-get clean  &&  apt-get autoclean && rm  /var/cache/apt/* -rf 	


WORKDIR /src
COPY ["IoTSharp/IoTSharp.csproj", "IoTSharp/"]
COPY ["IoTSharp.Data/IoTSharp.Data.csproj", "IoTSharp.Data/"]
COPY ["IoTSharp.Data.SqlServer/IoTSharp.Data.SqlServer.csproj", "IoTSharp.Data.SqlServer/"]
COPY ["IoTSharp.Data.Sqlite/IoTSharp.Data.Sqlite.csproj", "IoTSharp.Data.Sqlite/"]
COPY ["IoTSharp.Data.Oracle/IoTSharp.Data.Oracle.csproj", "IoTSharp.Data.Oracle/"]
COPY ["IoTSharp.Data.PostgreSQL/IoTSharp.Data.PostgreSQL.csproj", "IoTSharp.Data.PostgreSQL/"]
COPY ["IoTSharp.Data.MySQL/IoTSharp.Data.MySQL.csproj", "IoTSharp.Data.MySQL/"]
RUN dotnet restore "IoTSharp/IoTSharp.csproj"
COPY . .
WORKDIR "/src/IoTSharp"
RUN dotnet build "IoTSharp.csproj" -c Release -o /app/build
WORKDIR /web
COPY  ../ClientApp/Vue/package*.json  .
RUN  yarn install
COPY  ../ClientApp/Vue  .
RUN  yarn build
 

FROM build AS publish
RUN dotnet publish "IoTSharp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IoTSharp.dll"]