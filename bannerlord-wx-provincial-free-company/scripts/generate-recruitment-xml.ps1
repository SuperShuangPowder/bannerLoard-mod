[CmdletBinding()]
param(
    [string]$BannerlordGameDir,

    [string]$DesignPath = '',

    [switch]$SkipValidation
)

. (Join-Path $PSScriptRoot 'Common.ps1')

$projectRoot = Get-ProjectRoot
if ([string]::IsNullOrWhiteSpace($DesignPath)) {
    $DesignPath = Join-Path $projectRoot 'design\imperial_settlements.tsv'
}

# ---- 前置：设计源校验（生成前提，避免从错误数据产出运行时配置）----
if (-not $SkipValidation) {
    $validateScript = Join-Path $PSScriptRoot 'validate-design.ps1'
    try {
        & $validateScript -BannerlordGameDir $BannerlordGameDir -DesignPath $DesignPath -ErrorAction Stop
    }
    catch {
        throw "设计源校验未通过，拒绝生成运行时 XML：$($_.Exception.Message)"
    }
}

$rows = @(Read-DesignSource -Path $DesignPath)

# ---- 冻结映射（记录 032 §4 / 033 §3：地域→根兵只定义一次）----
$troopByRegion = @{
    North = 'NPCCharacter.wx_northern_borderlander'
    South = 'NPCCharacter.wx_southern_borderlander'
    West  = 'NPCCharacter.wx_western_borderlander'
    East  = 'NPCCharacter.wx_eastern_borderlander'
}
$chanceFormat = @{
    '0'  = '0'
    '30' = '0.30'
    '55' = '0.55'
    '80' = '0.80'
}

# ---- 116 个征兵点 = 19 城镇 + 97 村庄（25 座城堡仅作构建期锚点，不进入运行时）----
$recruitRows = @($rows | Where-Object { $_.type -ne 'castle' })
if ($recruitRows.Count -ne 116) {
    throw "征兵点数量应为 116，实际 $($recruitRows.Count)"
}

$builder = New-Object System.Text.StringBuilder
[void]$builder.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
[void]$builder.AppendLine('<!-- 生成物，请勿手改。由 scripts/generate-recruitment-xml.ps1 从 design/imperial_settlements.tsv 同源生成；')
[void]$builder.AppendLine('     运行时由 C# 配置加载器直接读取，不在 SubModule.xml 中注册 XmlNode。 -->')
[void]$builder.AppendLine('<RegionalRecruitment>')
[void]$builder.AppendLine('  <RegionMapping>')
foreach ($region in @('North', 'South', 'West', 'East')) {
    [void]$builder.AppendLine("    <Region region=`"$region`" troop=`"$($troopByRegion[$region])`" />")
}
[void]$builder.AppendLine('  </RegionMapping>')
[void]$builder.AppendLine('  <Settlements>')
foreach ($row in ($recruitRows | Sort-Object id)) {
    $chance = $chanceFormat[$row.chance_pct]
    if ($null -eq $chance) {
        throw "[$($row.id)] 概率 $($row.chance_pct) 无格式化映射"
    }
    [void]$builder.AppendLine("    <Settlement id=`"$($row.id)`" region=`"$($row.region)`" tier=`"$($row.tier)`" chance=`"$chance`" />")
}
[void]$builder.AppendLine('  </Settlements>')
[void]$builder.AppendLine('</RegionalRecruitment>')

$outPath = Join-Path $projectRoot 'src\WXProvincialFreeCompany\ModuleData\regional_recruitment.xml'
[System.IO.File]::WriteAllText($outPath, $builder.ToString(), (New-Object System.Text.UTF8Encoding($false)))

# ---- 生成后自检（重新解析，验证可读性与冻结约束）----
[xml]$check = $builder.ToString()
$nodes = @($check.RegionalRecruitment.Settlements.Settlement)
if ($nodes.Count -ne 116) {
    throw "生成后自检：征兵点 $($nodes.Count) ≠ 116"
}
$dupIds = @($nodes | Group-Object id | Where-Object { $_.Count -gt 1 })
if ($dupIds.Count -gt 0) {
    throw "生成后自检：重复 ID $($dupIds.Name -join ', ')"
}
if (@($nodes | Where-Object { $_.id -match '^castle_[A-Z][A-Z0-9]+$' }).Count -gt 0) {
    throw '生成后自检：运行时配置混入城堡行'
}
foreach ($node in $nodes) {
    if ($node.chance -notin @('0', '0.30', '0.55', '0.80')) {
        throw "生成后自检：非法概率 $($node.id)=$($node.chance)"
    }
    if ($node.region -eq 'Core' -and $node.chance -ne '0') {
        throw "生成后自检：Core 行概率非 0：$($node.id)"
    }
    if ($node.region -ne 'Core' -and $node.chance -eq '0') {
        throw "生成后自检：非 Core 征兵点概率为 0：$($node.id)"
    }
}
$mapping = @($check.RegionalRecruitment.RegionMapping.Region)
if ($mapping.Count -ne 4) {
    throw "生成后自检：RegionMapping 应为 4 条，实际 $($mapping.Count)"
}
foreach ($region in @('North', 'South', 'West', 'East')) {
    $entry = @($mapping | Where-Object { $_.region -eq $region })[0]
    if (-not $entry -or $entry.troop -ne $troopByRegion[$region]) {
        throw "生成后自检：$region 根兵映射错误"
    }
}

# ---- 汇总 ----
Write-Host ''
Write-Host '✓ 运行时地域配置已生成并通过自检：'
Write-Host "  文件:     $outPath"
Write-Host "  征兵点:   $($nodes.Count)（19 城镇 + 97 村庄；25 座城堡仅保留在构建期）"
Write-Host '  各地域分布（按层级）：'
foreach ($region in @('North', 'South', 'West', 'East', 'Core')) {
    $regionNodes = @($nodes | Where-Object { $_.region -eq $region })
    $byTier = ($regionNodes | Group-Object tier | ForEach-Object { "$($_.Name)=$($_.Count)" }) -join ', '
    if ($byTier -eq '') { $byTier = '—' }
    Write-Host ("    {0,-6} 小计 {1,3}：{2}" -f $region, $regionNodes.Count, $byTier)
}
