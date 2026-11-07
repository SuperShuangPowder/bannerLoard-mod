Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-ProjectRoot {
    return (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
}

function Resolve-BannerlordGameDirectory {
    param(
        [Parameter(Mandatory = $false)]
        [string]$BannerlordGameDir
    )

    if ([string]::IsNullOrWhiteSpace($BannerlordGameDir)) {
        $BannerlordGameDir = $env:BANNERLORD_GAME_DIR
    }

    if ([string]::IsNullOrWhiteSpace($BannerlordGameDir)) {
        throw 'BannerlordGameDir is required. Pass -BannerlordGameDir or set BANNERLORD_GAME_DIR.'
    }

    $resolved = (Resolve-Path -LiteralPath $BannerlordGameDir).Path
    $clientBin = Join-Path $resolved 'bin\Win64_Shipping_Client'
    $requiredAssemblies = @(
        'TaleWorlds.Library.dll',
        'TaleWorlds.Core.dll',
        'TaleWorlds.ObjectSystem.dll',
        'TaleWorlds.CampaignSystem.dll',
        'TaleWorlds.MountAndBlade.dll'
    )

    foreach ($assembly in $requiredAssemblies) {
        $assemblyPath = Join-Path $clientBin $assembly
        if (-not (Test-Path -LiteralPath $assemblyPath -PathType Leaf)) {
            throw "Required Bannerlord assembly not found: $assemblyPath"
        }
    }

    return $resolved
}

function Get-DotNetCommand {
    $projectRoot = Get-ProjectRoot
    $cliHome = Join-Path $projectRoot 'artifacts\dotnet-cli-home'
    New-Item -ItemType Directory -Path $cliHome -Force | Out-Null
    $env:DOTNET_CLI_HOME = $cliHome
    $env:DOTNET_NOLOGO = '1'
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'

    $dotnet = Get-Command dotnet -ErrorAction Stop
    $version = (& $dotnet.Source --version).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet --version failed.'
    }

    if ($version -ne '6.0.428') {
        throw "This project requires .NET SDK 6.0.428; resolved SDK is $version."
    }

    return $dotnet.Source
}

function Assert-NoOfficialAssemblies {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Directory
    )

    $forbidden = Get-ChildItem -LiteralPath $Directory -Recurse -File |
        Where-Object {
            $_.Name -like 'TaleWorlds*.dll' -or
            $_.Name -like 'NavalDLC*.dll'
        }

    if ($forbidden) {
        $names = ($forbidden.FullName -join [Environment]::NewLine)
        throw "Official game DLLs must not be copied into project output:`n$names"
    }
}

function Read-DesignSource {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Design source not found: $Path"
    }

    $rows = @()
    foreach ($line in [System.IO.File]::ReadAllLines($Path, [System.Text.Encoding]::UTF8)) {
        if ($line -match '^\s*#' -or $line.Trim().Length -eq 0) {
            continue
        }

        $fields = $line.Split([char[]]@([char]9), [System.StringSplitOptions]::None)
        if ($fields.Count -ne 11) {
            throw "Design source row must have 11 fields, got $($fields.Count): $line"
        }
        if ($fields[0] -eq 'id') {
            continue
        }

        $rows += [pscustomobject]@{
            id         = $fields[0]
            name_zh    = $fields[1]
            name_en    = $fields[2]
            type       = $fields[3]
            faction    = $fields[4]
            bound      = $fields[5]
            region     = $fields[6]
            tier       = $fields[7]
            chance_pct = $fields[8]
            pos_x      = $fields[9]
            pos_y      = $fields[10]
        }
    }

    return $rows
}
