param(
    [Parameter(Mandatory = $true)]
    [string]$PublishDir,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

function New-WixId {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Prefix,

        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $normalized = ($Value -replace '[^A-Za-z0-9_\.]', '_')

    if ([string]::IsNullOrWhiteSpace($normalized)) {
        $normalized = 'root'
    }

    if ($normalized[0] -match '[0-9]') {
        $normalized = "_$normalized"
    }

    $candidate = "$Prefix$normalized"

    if ($candidate.Length -le 60) {
        return $candidate
    }

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    $hashBytes = $sha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($candidate))
    $hash = ([System.BitConverter]::ToString($hashBytes) -replace '-', '').Substring(0, 12)
    $headLength = [Math]::Max(1, 60 - $Prefix.Length - $hash.Length - 1)
    $head = $normalized.Substring(0, [Math]::Min($normalized.Length, $headLength))
    return "$Prefix${head}_$hash"
}

function Escape-Xml {
    param([string]$Value)
    return [System.Security.SecurityElement]::Escape($Value)
}

$publishRoot = [System.IO.Path]::GetFullPath($PublishDir)

if (-not (Test-Path -LiteralPath $publishRoot)) {
    throw "Publish directory not found: $publishRoot"
}

$outputDirectory = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and -not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

$directories = Get-ChildItem -Path $publishRoot -Directory -Recurse |
    Sort-Object { $_.FullName.Length }

$directoryIdMap = @{
    $publishRoot = 'INSTALLDIR'
}

$directoryFragments = New-Object System.Collections.Generic.List[string]

foreach ($directory in $directories) {
    $parentPath = Split-Path -Parent $directory.FullName
    $parentId = $directoryIdMap[$parentPath]
    $directoryId = New-WixId -Prefix 'DIR_' -Value ($directory.FullName.Substring($publishRoot.Length).TrimStart('\'))
    $directoryIdMap[$directory.FullName] = $directoryId

    $directoryFragments.Add("    <DirectoryRef Id=`"$parentId`">")
    $directoryFragments.Add("      <Directory Id=`"$directoryId`" Name=`"$(Escape-Xml $directory.Name)`" />")
    $directoryFragments.Add("    </DirectoryRef>")
}

$componentRefs = New-Object System.Collections.Generic.List[string]
$componentFragments = New-Object System.Collections.Generic.List[string]
$files = Get-ChildItem -Path $publishRoot -File -Recurse | Sort-Object FullName

foreach ($file in $files) {
    $relativeDirectory = Split-Path -Parent ($file.FullName.Substring($publishRoot.Length).TrimStart('\'))
    $directoryKey = if ([string]::IsNullOrWhiteSpace($relativeDirectory)) { $publishRoot } else { Join-Path $publishRoot $relativeDirectory }
    $directoryId = $directoryIdMap[$directoryKey]
    $componentId = New-WixId -Prefix 'CMP_' -Value ($file.FullName.Substring($publishRoot.Length).TrimStart('\'))
    $fileId = New-WixId -Prefix 'FIL_' -Value ($file.FullName.Substring($publishRoot.Length).TrimStart('\'))
    $componentRefs.Add("      <ComponentRef Id=`"$componentId`" />")

    $componentFragments.Add("    <Component Id=`"$componentId`" Directory=`"$directoryId`" Guid=`"*`">")
    $componentFragments.Add("      <File Id=`"$fileId`" Source=`"$(Escape-Xml $file.FullName)`" KeyPath=`"yes`" />")

    if ($file.Name -ieq 'IoTSharp.exe') {
        $componentFragments.Add('      <ServiceInstall Id="IoTSharpServiceInstaller" Name="IoTSharp" DisplayName="IoTSharp" Description="IoTSharp Windows service" Type="ownProcess" Start="auto" ErrorControl="normal" Arguments="--environment Production" Vital="yes" />')
        $componentFragments.Add('      <ServiceControl Id="IoTSharpServiceController" Name="IoTSharp" Start="install" Stop="both" Remove="uninstall" Wait="yes" />')
    }

    $componentFragments.Add('    </Component>')
}

$content = @(
    '<Include xmlns="http://wixtoolset.org/schemas/v4/wxs">'
    '  <Fragment>'
    '    <ComponentGroup Id="PublishedFiles">'
)

$content += $componentRefs
$content += @(
    '    </ComponentGroup>'
    '  </Fragment>'
    '  <Fragment>'
)

$content += $directoryFragments
$content += @(
    '  </Fragment>'
    '  <Fragment>'
    '    <DirectoryRef Id="INSTALLDIR">'
)

$content += $componentFragments
$content += @(
    '    </DirectoryRef>'
    '  </Fragment>'
    '</Include>'
)

[System.IO.File]::WriteAllLines($OutputPath, $content)
