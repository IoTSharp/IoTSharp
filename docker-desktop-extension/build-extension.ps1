param(
    [string]$ImageName = "iotsharp/iotsharp-dd-extension:0.1.0",
    [switch]$SkipPublish,
    [switch]$FullValidate
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $PSScriptRoot 'build\publish'

if (-not $SkipPublish) {
    if (Test-Path $publishDir) {
        Remove-Item -Recurse -Force $publishDir
    }

    New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

    Push-Location $root
    try {
        dotnet publish .\IoTSharp\IoTSharp.csproj -c Release -o $publishDir /p:UseAppHost=false
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet publish failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

Push-Location $root
try {
    docker extension validate .\docker-desktop-extension\metadata.json
    if ($LASTEXITCODE -ne 0) {
        throw "metadata validation failed with exit code $LASTEXITCODE"
    }

    docker build -f docker-desktop-extension/Dockerfile -t $ImageName .
    if ($LASTEXITCODE -ne 0) {
        throw "docker build failed with exit code $LASTEXITCODE"
    }

    if ($FullValidate) {
        docker extension validate $ImageName
        if ($LASTEXITCODE -ne 0) {
            throw "docker extension validate failed with exit code $LASTEXITCODE"
        }
    }
}
finally {
    Pop-Location
}
