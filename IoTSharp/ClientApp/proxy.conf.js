const { env } = require('process');

const target = env.ASPNETCORE_HTTP_PORT ? `http://localhost:${env.ASPNETCORE_HTTP_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:5000';

const PROXY_CONFIG = [
  {
    context: [
      "/_configuration",
      "/.well-known",
      "/Identity",
      "/connect",
      "/ApplyDatabaseMigrations",
      "/_framework",
      "/swagger",
      "/quartz",
      "/cap",
      "/rin",
      "/healthchecks-ui",
      "/api",
      "/healthz",
      "/api/healthchecks"
    ],
    target: target,
    secure: false,
    headers: {
      Connection: 'Keep-Alive'
    }
  }
]

module.exports = PROXY_CONFIG;
