[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$BannerlordGameDir,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [switch]$SkipBuild,

    [bool]$IncludeEditor = $true
)

. (Join-Path $PSScriptRoot 'Common.ps1')

$projectRoot = Get-ProjectRoot
$gameDir = Resolve-BannerlordGameDirectory -BannerlordGameDir $BannerlordGameDir
$moduleSource = Join-Path $projectRoot 'src\WXProvincialFreeCompany'
$moduleTarget = Join-Path $gameDir 'Modules\WXProvincialFreeCompany'
$buildOutput = Join-Path $projectRoot "artifacts\build\$Configuration"

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

New-Item -ItemType Directory -Path $moduleTarget -Force | Out-Null
Get-ChildItem -LiteralPath $moduleSource -Force | Copy-Item -Destination $moduleTarget -Recurse -Force

$runtimeDirectories = @('bin\Win64_Shipping_Client')
if ($IncludeEditor) {
    $runtimeDirectories += 'bin\Win64_Shipping_wEditor'
}

foreach ($relativeDirectory in $runtimeDirectories) {
    $runtimeTarget = Join-Path $moduleTarget $relativeDirectory
    New-Item -ItemType Directory -Path $runtimeTarget -Force | Out-Null
    Copy-Item -LiteralPath $moduleDll -Destination $runtimeTarget -Force

    $pdb = Join-Path $buildOutput 'WXProvincialFreeCompany.pdb'
    if (Test-Path -LiteralPath $pdb -PathType Leaf) {
        Copy-Item -LiteralPath $pdb -Destination $runtimeTarget -Force
    }
}

$sourceHash = (Get-FileHash -LiteralPath $moduleDll -Algorithm SHA256).Hash
foreach ($relativeDirectory in $runtimeDirectories) {
    $deployedDll = Join-Path (Join-Path $moduleTarget $relativeDirectory) 'WXProvincialFreeCompany.dll'
    $deployedHash = (Get-FileHash -LiteralPath $deployedDll -Algorithm SHA256).Hash
    if ($deployedHash -ne $sourceHash) {
        throw "Deployed DLL hash mismatch: $deployedDll"
    }
}

Assert-NoOfficialAssemblies -Directory $moduleTarget

Write-Host ''
Write-Host 'Deployment verified:'
Write-Host "  Module:        $moduleTarget"
Write-Host "  DLL SHA-256:   $sourceHash"
Write-Host "  Runtime bins:  $($runtimeDirectories -join ', ')"
Write-Host '  Official DLLs: 0 copied'
