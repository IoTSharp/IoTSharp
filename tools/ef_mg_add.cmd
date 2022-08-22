echo off
cls
echo Add  a migration  name is %2  to  IoTSharp.Data.%1
set ASPNETCORE_ENVIRONMENT=%1
dotnet dotnet-ef  migrations  add  %2  --context IoTSharp.Data.ApplicationDbContext   --startup-project  ..\IoTSharp\IoTSharp.csproj  --project ..\IoTSharp.Data.%1\IoTSharp.Data.%1.csproj