[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$BannerlordGameDir,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

. (Join-Path $PSScriptRoot 'Common.ps1')

$projectRoot = Get-ProjectRoot
$gameDir = Resolve-BannerlordGameDirectory -BannerlordGameDir $BannerlordGameDir
$dotnet = Get-DotNetCommand
$projectFile = Join-Path $projectRoot 'code\WXProvincialFreeCompany\WXProvincialFreeCompany.csproj'
$outputDirectory = Join-Path $projectRoot "artifacts\build\$Configuration"

Push-Location $projectRoot
try {
    & $dotnet restore $projectFile "-p:BannerlordGameDir=$gameDir"
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet restore failed.'
    }

    & $dotnet build $projectFile --configuration $Configuration --no-restore "-p:BannerlordGameDir=$gameDir"
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet build failed.'
    }
}
finally {
    Pop-Location
}

$moduleDll = Join-Path $outputDirectory 'WXProvincialFreeCompany.dll'
if (-not (Test-Path -LiteralPath $moduleDll -PathType Leaf)) {
    throw "Expected build output was not created: $moduleDll"
}

Assert-NoOfficialAssemblies -Directory $outputDirectory
$hash = (Get-FileHash -LiteralPath $moduleDll -Algorithm SHA256).Hash

Write-Host ''
Write-Host 'Build verified:'
Write-Host "  SDK:           6.0.428"
Write-Host "  Configuration: $Configuration"
Write-Host "  DLL:           $moduleDll"
Write-Host "  SHA-256:       $hash"
Write-Host '  Official DLLs: 0 copied'
