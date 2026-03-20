param(
    [Parameter(Mandatory = $true)]
    [string]$InstallDir,

    [Parameter(Mandatory = $true)]
    [string]$Template,

    [string]$Host,
    [string]$Port,
    [string]$Database,
    [string]$Username,
    [string]$Password,
    [string]$SqlitePath
)

$ErrorActionPreference = 'Stop'

function Get-CanonicalTemplate {
    param([string]$Value)

    switch ($Value.ToLowerInvariant()) {
        'sqlite' { return 'Sqlite' }
        'postgresql' { return 'PostgreSql' }
        'mysql' { return 'MySql' }
        'sqlserver' { return 'SQLServer' }
        'oracle' { return 'Oracle' }
        'clickhouse' { return 'ClickHouse' }
        'cassandra' { return 'Cassandra' }
        default { throw "Unsupported database template: $Value" }
    }
}

function Get-DefaultPort {
    param([string]$CanonicalTemplate)

    switch ($CanonicalTemplate) {
        'PostgreSql' { return '5432' }
        'MySql' { return '3306' }
        'SQLServer' { return '1433' }
        'Oracle' { return '1521' }
        'ClickHouse' { return '8123' }
        'Cassandra' { return '9042' }
        default { return '' }
    }
}

function Add-IfValue {
    param(
        [System.Collections.Generic.List[string]]$Parts,
        [string]$Template,
        [string]$Value
    )

    if (-not [string]::IsNullOrWhiteSpace($Value)) {
        $Parts.Add("$Template=$Value")
    }
}

$canonicalTemplate = Get-CanonicalTemplate -Value $Template
$installRoot = [System.IO.Path]::GetFullPath($InstallDir)
$databaseName = if ([string]::IsNullOrWhiteSpace($Database)) { 'IoTSharp' } else { $Database.Trim() }
$serverHost = if ([string]::IsNullOrWhiteSpace($Host)) { 'localhost' } else { $Host.Trim() }
$serverPort = if ([string]::IsNullOrWhiteSpace($Port)) { Get-DefaultPort -CanonicalTemplate $canonicalTemplate } else { $Port.Trim() }
$sqliteValue = if ([string]::IsNullOrWhiteSpace($SqlitePath)) { '.data\IoTSharp.db' } else { $SqlitePath.Trim() }

Get-ChildItem -LiteralPath $installRoot -Filter 'appsettings.*.Installer.json' -File -ErrorAction SilentlyContinue |
    Remove-Item -Force -ErrorAction SilentlyContinue

if ($canonicalTemplate -eq 'Sqlite') {
    $sqliteFullPath = if ([System.IO.Path]::IsPathRooted($sqliteValue)) {
        $sqliteValue
    }
    else {
        Join-Path $installRoot $sqliteValue
    }

    $sqliteDirectory = Split-Path -Parent $sqliteFullPath
    if (-not [string]::IsNullOrWhiteSpace($sqliteDirectory)) {
        New-Item -ItemType Directory -Path $sqliteDirectory -Force | Out-Null
    }

    $connectionString = "Data Source=$sqliteValue"
}
elseif ($canonicalTemplate -eq 'MySql') {
    $connectionString = "server=$serverHost;port=$serverPort;user=$Username;password=$Password;database=$databaseName"
}
elseif ($canonicalTemplate -eq 'PostgreSql') {
    $connectionString = "Server=$serverHost;Port=$serverPort;Database=$databaseName;Username=$Username;Password=$Password;Include Error Detail=true;"
}
elseif ($canonicalTemplate -eq 'SQLServer') {
    $dataSource = if ([string]::IsNullOrWhiteSpace($serverPort)) { $serverHost } else { "$serverHost,$serverPort" }
    $connectionString = "Data Source=$dataSource;Initial Catalog=$databaseName;Persist Security Info=True;User ID=$Username;Password=$Password;TrustServerCertificate=true"
}
elseif ($canonicalTemplate -eq 'Oracle') {
    $oracleService = if ([string]::IsNullOrWhiteSpace($databaseName)) { 'xe' } else { $databaseName }
    $connectionString = "DATA SOURCE=$serverHost:$serverPort/$oracleService;PASSWORD=$Password;PERSIST SECURITY INFO=True;USER ID=$Username;"
}
elseif ($canonicalTemplate -eq 'ClickHouse') {
    $connectionString = "Host=$serverHost;Protocol=http;Port=$serverPort;Database=$databaseName;Username=$Username;Password=$Password"
}
else {
    $segments = [System.Collections.Generic.List[string]]::new()
    Add-IfValue -Parts $segments -Template 'Contact Points' -Value $serverHost
    Add-IfValue -Parts $segments -Template 'Port' -Value $serverPort
    Add-IfValue -Parts $segments -Template 'Username' -Value $Username
    Add-IfValue -Parts $segments -Template 'Password' -Value $Password
    Add-IfValue -Parts $segments -Template 'Default Keyspace' -Value $databaseName
    $connectionString = ($segments -join ';') + ';'
}

$config = [ordered]@{
    DataBase = $canonicalTemplate
    ConnectionStrings = [ordered]@{
        IoTSharp = $connectionString
    }
}

$targetFile = Join-Path $installRoot "appsettings.$canonicalTemplate.Installer.json"
$json = $config | ConvertTo-Json -Depth 5
[System.IO.File]::WriteAllText($targetFile, $json, [System.Text.UTF8Encoding]::new($false))
