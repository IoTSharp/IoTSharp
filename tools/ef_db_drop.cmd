echo off
cls
echo Drop  to  IoTSharp.Data.%1
set ASPNETCORE_ENVIRONMENT=%1
dotnet dotnet-ef  database drop      --context IoTSharp.Data.ApplicationDbContext   --startup-project  ..\IoTSharp\IoTSharp.csproj  --project ..\IoTSharp.Data.%1\IoTSharp.Data.%1.csproj