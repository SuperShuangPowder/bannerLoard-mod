[CmdletBinding()]
param(
    [string]$DesignPath = '',

    [string]$MapHtmlPath = ''
)

. (Join-Path $PSScriptRoot 'Common.ps1')

$projectRoot = Get-ProjectRoot
if ([string]::IsNullOrWhiteSpace($DesignPath)) {
    $DesignPath = Join-Path $projectRoot 'design\imperial_settlements.tsv'
}
if ([string]::IsNullOrWhiteSpace($MapHtmlPath)) {
    $MapHtmlPath = Join-Path $projectRoot '..\..\帝国地域征募分区地图-世界观校准版.html'
}

$rows = @(Read-DesignSource -Path $DesignPath)
if ($rows.Count -ne 141) {
    throw "设计源数据行数应为 141，实际 $($rows.Count)"
}
if (-not (Test-Path -LiteralPath $MapHtmlPath -PathType Leaf)) {
    throw "审阅地图 HTML 不存在: $MapHtmlPath"
}

# 从设计源同源重建 HTML 的 const data 数组（记录 033 §3：地图与运行时 XML 同源生成，不从 HTML 反向抓数据）。
$regionLower = @{ Core = 'core'; North = 'north'; South = 'south'; West = 'west'; East = 'east' }
$tierLower = @{ Core = 'core'; Transition = 'transition'; Border = 'border'; Frontier = 'frontier' }
$tierChance = @{ Core = '0'; Transition = '20'; Border = '30'; Frontier = '50' }

$parts = New-Object System.Collections.Generic.List[string]
foreach ($row in $rows) {
    $nameZh = $row.name_zh -replace '\\', '\\' -replace '"', '\"'
    $nameEn = $row.name_en -replace '\\', '\\' -replace '"', '\"'
    $p = if ($row.type -eq 'castle') { $tierChance[$row.tier] } else { $row.chance_pct }
    $obj = '{{"id":"{0}","n":"{1}","en":"{2}","x":{3},"y":{4},"t":"{5}","f":"{6}","b":"{7}","r":"{8}","z":"{9}","p":{10}}}' -f `
        $row.id, $nameZh, $nameEn, $row.pos_x, $row.pos_y, $row.type, $row.faction, $row.bound, `
        $regionLower[$row.region], $tierLower[$row.tier], $p
    $parts.Add($obj)
}
$json = '[' + ($parts -join ',') + ']'

$html = Get-Content -LiteralPath $MapHtmlPath -Raw -Encoding UTF8
$newHtml = [regex]::Replace(
    $html,
    'const\s+data\s*=\s*\[.*?\]\s*;',
    ('const data = ' + $json + ';'),
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if ($newHtml -eq $html) {
    throw '未能在 HTML 中找到可替换的 const data 数组。'
}
[System.IO.File]::WriteAllText($MapHtmlPath, $newHtml, (New-Object System.Text.UTF8Encoding($false)))

Write-Host ''
Write-Host '✓ 审阅地图数据已由设计源同源重建：'
Write-Host "  文件:     $MapHtmlPath"
Write-Host "  对象数:   $($rows.Count)（19 城镇 + 25 城堡 + 97 村庄）"
Write-Host "  档位:     核心 0 / 过渡 20 / 腹地 30 / 前线 50"
