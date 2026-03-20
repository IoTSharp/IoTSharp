FROM mcr.microsoft.com/dotnet/aspnet:10.0

LABEL org.opencontainers.image.title="IoTSharp" \
      org.opencontainers.image.description="Launch an IoTSharp demo stack inside Docker Desktop with a single install." \
      org.opencontainers.image.vendor="IoTSharp" \
      org.opencontainers.image.source="https://github.com/IoTSharp/IoTSharp" \
      com.docker.desktop.extension.api.version="0.3.0" \
      com.docker.extension.categories="cloud,utility-tools" \
      com.docker.extension.publisher-url="https://github.com/IoTSharp/IoTSharp" \
      com.docker.extension.changelog="<ul><li>Initial Docker Desktop extension preview for IoTSharp.</li><li>Ships a one-click SQLite demo stack powered by the extension image itself.</li></ul>" \
      com.docker.extension.detailed-description="<p>Install IoTSharp as a Docker Desktop extension to start a lightweight SQLite-backed demo in minutes.</p><ul><li>Self-contained extension image with built-in IoTSharp runtime</li><li>Auto-started demo stack via embedded Docker Compose</li><li>Quick links for first launch, ports, data persistence, and next-step docs</li></ul>" \
      com.docker.extension.screenshots="[{\"url\":\"https://raw.githubusercontent.com/IoTSharp/IoTSharp/master/docs/static/img/iotsharp/iotsharp-dashboard.png\",\"alt\":\"IoTSharp dashboard\"},{\"url\":\"https://raw.githubusercontent.com/IoTSharp/IoTSharp/master/docs/static/img/iotsharp/docker-run.png\",\"alt\":\"Docker deployment experience\"}]" \
      com.docker.desktop.extension.icon="https://raw.githubusercontent.com/IoTSharp/IoTSharp/master/docs/static/img/96x96.png"

WORKDIR /opt/iotsharp
COPY docker-desktop-extension/build/publish/ ./

WORKDIR /
COPY docker-desktop-extension/metadata.json /metadata.json
COPY docker-desktop-extension/vm/extension-compose.yml /vm/extension-compose.yml
COPY docker-desktop-extension/ui /ui
COPY docs/static/img/96x96.png /icon.png

CMD ["sh", "-c", "echo 'IoTSharp Docker Desktop extension package image'; sleep infinity"]
