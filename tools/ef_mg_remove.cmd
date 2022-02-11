echo off
cls
echo remove  a migration  from  IoTSharp.Data.%1
set ASPNETCORE_ENVIRONMENT=%1
dotnet ef  migrations  remove   --context IoTSharp.Data.ApplicationDbContext   --startup-project  ..\IoTSharp\IoTSharp.csproj  --project ..\IoTSharp.Data.%1\IoTSharp.Data.%1.csproj