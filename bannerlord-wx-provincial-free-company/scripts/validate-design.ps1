[CmdletBinding()]
param(
    [string]$BannerlordGameDir,

    [switch]$SkipGameCheck,

    [switch]$SkipMapCheck,

    [string]$DesignPath = '',

    [string]$MapHtmlPath = ''
)

. (Join-Path $PSScriptRoot 'Common.ps1')

$projectRoot = Get-ProjectRoot
if ([string]::IsNullOrWhiteSpace($DesignPath)) {
    $DesignPath = Join-Path $projectRoot 'design\imperial_settlements.tsv'
}

$rows = @(Read-DesignSource -Path $DesignPath)
$errors = New-Object System.Collections.Generic.List[string]

function Fail([string]$Message) {
    $script:errors.Add($Message)
}

# ---- 1. 数量与唯一性 ----
if ($rows.Count -ne 141) {
    Fail "设计源数据行数应为 141，实际 $($rows.Count)"
}
$duplicates = @($rows | Group-Object id | Where-Object { $_.Count -gt 1 })
foreach ($dup in $duplicates) {
    Fail "重复 ID: $($dup.Name) x$($dup.Count)"
}

# ---- 2. 类型分布（19 城镇 + 25 城堡 + 97 村庄）----
$byType = @($rows | Group-Object type)
foreach ($want in @(@('town', 19), @('castle', 25), @('village', 97))) {
    $got = @($byType | Where-Object { $_.Name -eq $want[0] } | ForEach-Object { $_.Count })
    if (@($got).Count -eq 0) { $got = @(0) }
    if ($got[0] -ne $want[1]) {
        Fail "类型 $($want[0]) 数量应为 $($want[1])，实际 $($got[0])"
    }
}

# ---- 3. 字段规则、绑定与继承一致性 ----
$ids = @($rows | ForEach-Object { $_.id })
$validRegion = @('North', 'South', 'West', 'East', 'Core')
$validTier = @('Core', 'Transition', 'Border', 'Frontier')
$validChance = @('0', '20', '30', '50')

foreach ($row in $rows) {
    if ($row.region -notin $validRegion) {
        Fail "[$($row.id)] 非法地域 $($row.region)"
    }
    if ($row.tier -notin $validTier) {
        Fail "[$($row.id)] 非法层级 $($row.tier)"
    }

    if ($row.type -eq 'castle') {
        if ($row.chance_pct -ne '') { Fail "[$($row.id)] 城堡不应有概率（当前 $($row.chance_pct)）" }
        if ($row.bound -ne '') { Fail "[$($row.id)] 城堡不应有绑定（当前 $($row.bound)）" }
    }
    elseif ($row.type -eq 'town') {
        if ($row.chance_pct -notin $validChance) { Fail "[$($row.id)] 城镇概率非法: $($row.chance_pct)" }
        if ($row.bound -ne '') { Fail "[$($row.id)] 城镇不应有绑定（当前 $($row.bound)）" }
    }
    elseif ($row.type -eq 'village') {
        if ($row.chance_pct -notin $validChance) { Fail "[$($row.id)] 村庄概率非法: $($row.chance_pct)" }
        if ($row.bound -eq '') { Fail "[$($row.id)] 村庄缺少绑定" }
        elseif ($row.bound -notin $ids) { Fail "[$($row.id)] 绑定目标不存在: $($row.bound)" }
        else {
            $anchor = @($rows | Where-Object { $_.id -eq $row.bound })[0]
            if ($anchor.type -notin @('town', 'castle')) {
                Fail "[$($row.id)] 绑定目标类型非法: $($anchor.type)"
            }
            # 记录 032 §1：村庄继承其绑定城镇或城堡的地域、层级与概率
            if ($anchor.region -ne $row.region) {
                Fail "[$($row.id)] 村庄地域 $($row.region) 与锚点 $($anchor.id) 的 $($anchor.region) 不一致"
            }
            if ($anchor.tier -ne $row.tier) {
                Fail "[$($row.id)] 村庄层级 $($row.tier) 与锚点 $($anchor.id) 的 $($anchor.tier) 不一致"
            }
            if ($anchor.type -eq 'town' -and $anchor.chance_pct -ne $row.chance_pct) {
                Fail "[$($row.id)] 村庄概率 $($row.chance_pct) 与锚点城镇 $($anchor.id) 的 $($anchor.chance_pct) 不一致"
            }
        }
    }
    else {
        Fail "[$($row.id)] 非法类型 $($row.type)"
    }

    # 核心区必须为 0%，非核心征兵点不能为 0%
    if ($row.type -ne 'castle') {
        if ($row.region -eq 'Core' -and $row.chance_pct -ne '0') {
            Fail "[$($row.id)] 核心区概率必须为 0（当前 $($row.chance_pct)）"
        }
        if ($row.region -ne 'Core' -and $row.chance_pct -eq '0') {
            Fail "[$($row.id)] 非核心征兵点概率不能为 0"
        }
    }
}

# ---- 4. 对照真实游戏数据（settlements.xml）----
if (-not $SkipGameCheck) {
    $gameDir = $null
    try {
        $gameDir = Resolve-BannerlordGameDirectory -BannerlordGameDir $BannerlordGameDir
    }
    catch {
        Write-Warning "跳过游戏数据核对：$($_.Exception.Message)"
    }

    if ($gameDir) {
        $settlementsXml = Join-Path $gameDir 'Modules\SandBox\ModuleData\settlements.xml'
        if (-not (Test-Path -LiteralPath $settlementsXml -PathType Leaf)) {
            Fail "游戏 settlements.xml 不存在: $settlementsXml"
        }
        else {
            [xml]$gameData = Get-Content -LiteralPath $settlementsXml -Raw -Encoding UTF8
            $gameSettlements = @{}
            foreach ($settlement in @($gameData.Settlements.Settlement)) {
                $gameSettlements[$settlement.id] = $settlement
            }

            foreach ($row in $rows) {
                $game = $gameSettlements[$row.id]
                if (-not $game) {
                    Fail "[$($row.id)] 游戏数据中不存在"; continue
                }
                if ($game.culture -notmatch 'empire') {
                    Fail "[$($row.id)] 游戏文化=$($game.culture)，非帝国"
                }
                $englishName = ($game.name -replace '^\{=[^}]*\}', '')
                if ($englishName -ne $row.name_en) {
                    Fail "[$($row.id)] 英文名不匹配：设计源='$($row.name_en)' 游戏='$englishName'"
                }
                if ([math]::Abs([double]$game.posX - [double]$row.pos_x) -gt 0.01 -or
                    [math]::Abs([double]$game.posY - [double]$row.pos_y) -gt 0.01) {
                    Fail "[$($row.id)] 坐标偏差：设计源=($($row.pos_x),$($row.pos_y)) 游戏=($($game.posX),$($game.posY))"
                }

                if ($row.type -eq 'village') {
                    $villageNode = $game.Components.SelectSingleNode('Village')
                    if (-not $villageNode) {
                        Fail "[$($row.id)] 游戏数据缺少 Village 组件"
                    }
                    else {
                        $bound = $villageNode.GetAttribute('bound')
                        if ($bound -ne ("Settlement." + $row.bound)) {
                            Fail "[$($row.id)] 游戏绑定=$bound，设计源=Settlement.$($row.bound)"
                        }
                    }
                }
                elseif ($row.type -eq 'castle') {
                    $townNode = $game.Components.SelectSingleNode('Town')
                    if (-not $townNode -or $townNode.GetAttribute('is_castle') -ne 'true') {
                        Fail "[$($row.id)] 游戏数据不是城堡（is_castle!=true）"
                    }
                }
                else {
                    if ($game.Components.SelectSingleNode('Village')) {
                        Fail "[$($row.id)] 游戏数据含 Village 组件，与 town 不符"
                    }
                    $townNode = $game.Components.SelectSingleNode('Town')
                    if ($townNode -and $townNode.GetAttribute('is_castle') -eq 'true') {
                        Fail "[$($row.id)] 游戏数据 is_castle=true，与 town 不符"
                    }
                }
            }

            # 反向检查：游戏帝国文化对象与设计源的差集必须恰为特殊聚落 retirement_retreat
            $gameEmpireIds = @($gameData.Settlements.Settlement |
                Where-Object { $_.culture -match 'empire' } |
                ForEach-Object { $_.id })
            $extra = @($gameEmpireIds | Where-Object { $_ -notin $ids })
            if (@($extra).Count -ne 1 -or @($extra)[0] -ne 'retirement_retreat') {
                Fail "游戏帝国文化对象与设计源差集应为仅 retirement_retreat，实际: $($extra -join ', ')"
            }
        }
    }
}

# ---- 5. 与审阅地图 HTML 的漂移检测 ----
if (-not $SkipMapCheck) {
    if ([string]::IsNullOrWhiteSpace($MapHtmlPath)) {
        $MapHtmlPath = Join-Path $projectRoot '..\..\帝国地域征募分区地图-世界观校准版.html'
    }
    if (-not (Test-Path -LiteralPath $MapHtmlPath -PathType Leaf)) {
        Write-Warning "未找到审阅地图 HTML，跳过漂移检测：$MapHtmlPath"
    }
    else {
        $html = Get-Content -LiteralPath $MapHtmlPath -Raw -Encoding UTF8
        $match = [regex]::Match($html, 'const\s+data\s*=\s*(\[.*?\])\s*;',
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
        if (-not $match.Success) {
            Fail 'HTML 中未找到 const data 数组'
        }
        else {
            $mapRows = [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value) | ConvertFrom-Json
            if ($mapRows.Count -ne 141) {
                Fail "HTML 对象数应为 141，实际 $($mapRows.Count)"
            }
            $mapById = @{}
            foreach ($mapRow in $mapRows) {
                $mapById[[string]$mapRow.id] = $mapRow
            }
            $regionReverse = @{ Core = 'core'; North = 'north'; South = 'south'; West = 'west'; East = 'east' }
            $tierReverse = @{ Core = 'core'; Transition = 'transition'; Border = 'border'; Frontier = 'frontier' }

            foreach ($row in $rows) {
                $mapRow = $mapById[$row.id]
                if (-not $mapRow) {
                    Fail "[$($row.id)] 设计源对象在 HTML 中缺失"; continue
                }
                if ([string]$mapRow.n -ne $row.name_zh) { Fail "[$($row.id)] 中文名漂移" }
                if ([string]$mapRow.en -ne $row.name_en) { Fail "[$($row.id)] 英文名漂移" }
                if ([string]$mapRow.t -ne $row.type) { Fail "[$($row.id)] 类型漂移" }
                if ([string]$mapRow.f -ne $row.faction) { Fail "[$($row.id)] 政权漂移" }
                if ([string]$mapRow.b -ne $row.bound) { Fail "[$($row.id)] 绑定漂移" }
                if ($regionReverse[$row.region] -ne [string]$mapRow.r) { Fail "[$($row.id)] 地域漂移" }
                if ($tierReverse[$row.tier] -ne [string]$mapRow.z) { Fail "[$($row.id)] 层级漂移" }
                if ($row.type -ne 'castle' -and [string]$mapRow.p -ne $row.chance_pct) {
                    Fail "[$($row.id)] 概率漂移：HTML=$($mapRow.p) 设计源=$($row.chance_pct)"
                }
                if ([string]$mapRow.x -ne $row.pos_x -or [string]$mapRow.y -ne $row.pos_y) {
                    Fail "[$($row.id)] 坐标漂移"
                }
            }

            $htmlExtra = @($mapRows | Where-Object { ([string]$_.id) -notin $ids })
            if (@($htmlExtra).Count -gt 0) {
                Fail "HTML 中存在设计源没有的对象: $(($htmlExtra | ForEach-Object { $_.id }) -join ', ')"
            }
        }
    }
}

# ---- 汇总 ----
if ($errors.Count -gt 0) {
    Write-Host ''
    Write-Host "✗ 设计源校验失败，共 $($errors.Count) 项：" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "  - $error" -ForegroundColor Red
    }
    throw 'Design source validation failed.'
}

Write-Host ''
Write-Host '✓ 设计源校验通过：'
Write-Host "  对象:      $($rows.Count) (19 城镇 + 25 城堡 + 97 村庄)"
Write-Host '  字段规则:  通过'
Write-Host "  游戏核对:  $(if ($SkipGameCheck) { '跳过' } elseif ($gameDir) { '通过 (settlements.xml)' } else { '跳过（无游戏目录）' })"
Write-Host "  地图漂移:  $(if ($SkipMapCheck) { '跳过' } elseif (Test-Path -LiteralPath $MapHtmlPath) { '通过' } else { '跳过（未找到 HTML）' })"
