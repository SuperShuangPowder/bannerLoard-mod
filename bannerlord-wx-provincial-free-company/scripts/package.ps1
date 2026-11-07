[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$BannerlordGameDir,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [switch]$SkipBuild
)

. (Join-Path $PSScriptRoot 'Common.ps1')

$projectRoot = Get-ProjectRoot
$gameDir = Resolve-BannerlordGameDirectory -BannerlordGameDir $BannerlordGameDir
$packageRoot = Join-Path $projectRoot 'artifacts\package'
$stageRoot = Join-Path $packageRoot 'staging'
$stageModule = Join-Path $stageRoot 'WXProvincialFreeCompany'
$moduleSource = Join-Path $projectRoot 'src\WXProvincialFreeCompany'
$buildOutput = Join-Path $projectRoot "artifacts\build\$Configuration"
$archive = Join-Path $packageRoot 'WXProvincialFreeCompany-v0.5.0.zip'

if (-not $SkipBuild) {
    & (Join-Path $PSScriptRoot 'build.ps1') -BannerlordGameDir $gameDir -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw 'Build script failed.'
    }
}

$moduleDll = Join-Path $buildOutput 'WXProvincialFreeCompany.dll'
if (-not (Test-Path -LiteralPath $moduleDll -PathType Leaf)) {
    throw "Build output not found: $moduleDll"
}

if (Test-Path -LiteralPath $stageRoot) {
    Remove-Item -LiteralPath $stageRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $stageModule -Force | Out-Null
Get-ChildItem -LiteralPath $moduleSource -Force | Copy-Item -Destination $stageModule -Recurse -Force

$clientBin = Join-Path $stageModule 'bin\Win64_Shipping_Client'
$editorBin = Join-Path $stageModule 'bin\Win64_Shipping_wEditor'
New-Item -ItemType Directory -Path $clientBin -Force | Out-Null
New-Item -ItemType Directory -Path $editorBin -Force | Out-Null
Copy-Item -LiteralPath $moduleDll -Destination $clientBin -Force
Copy-Item -LiteralPath $moduleDll -Destination $editorBin -Force

$pdb = Join-Path $buildOutput 'WXProvincialFreeCompany.pdb'
if (Test-Path -LiteralPath $pdb -PathType Leaf) {
    Copy-Item -LiteralPath $pdb -Destination $clientBin -Force
    Copy-Item -LiteralPath $pdb -Destination $editorBin -Force
}

Assert-NoOfficialAssemblies -Directory $stageModule

if (Test-Path -LiteralPath $archive) {
    Remove-Item -LiteralPath $archive -Force
}
Compress-Archive -LiteralPath $stageModule -DestinationPath $archive -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($archive)
try {
    $entryNames = @($zip.Entries | ForEach-Object FullName)
    $requiredEntries = @(
        'WXProvincialFreeCompany/SubModule.xml',
        'WXProvincialFreeCompany/bin/Win64_Shipping_Client/WXProvincialFreeCompany.dll'
    )
    foreach ($requiredEntry in $requiredEntries) {
        if ($entryNames -notcontains $requiredEntry) {
            throw "Package is missing required entry: $requiredEntry"
        }
    }

    $forbiddenEntries = @($entryNames | Where-Object {
        $_ -match '(^|/)(TaleWorlds|NavalDLC).*\.dll$'
    })
    if ($forbiddenEntries.Count -gt 0) {
        throw "Package contains official DLLs: $($forbiddenEntries -join ', ')"
    }
}
finally {
    $zip.Dispose()
}

$archiveHash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
Write-Host ''
Write-Host 'Package verified:'
Write-Host "  Archive:       $archive"
Write-Host "  SHA-256:       $archiveHash"
Write-Host '  Official DLLs: 0 included'
