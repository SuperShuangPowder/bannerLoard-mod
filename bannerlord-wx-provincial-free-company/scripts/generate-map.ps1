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
$dataPattern = 'const\s+data\s*=\s*(\[.*?\])\s*;'
if (-not [regex]::IsMatch($html, $dataPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
    throw '未能在 HTML 中找到可替换的 const data 数组。'
}

# 该 HTML 是实体转义序列化格式（脚本与数据均以 &lt;/&quot;/&#x27; 存储）。
# 新 JSON 必须按同一编码写入，裸引号会撕裂外层转义块导致页面无法渲染。
$escapedJson = $json -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;' -replace '"', '&quot;'
$newHtml = [regex]::Replace(
    $html,
    $dataPattern,
    ('const data = ' + $escapedJson + ';'),
    [System.Text.RegularExpressions.RegexOptions]::Singleline)

# 图例与说明文字中的档位是硬编码的，必须与数据同源更新（教训见记录 040 事故记录）。
$tPct = @{ Transition = $tierChance['Transition']; Border = $tierChance['Border']; Frontier = $tierChance['Frontier'] }
$newHtml = [regex]::Replace(
    $newHtml,
    '过渡圈 \d+% · 边境腹地 \d+% · 外缘前线 \d+%',
    "过渡圈 $($tPct.Transition)% · 边境腹地 $($tPct.Border)% · 外缘前线 $($tPct.Frontier)%")
foreach ($label in @('过渡圈', '边境腹地', '外缘前线')) {
    $value = if ($label -eq '过渡圈') { $tPct.Transition } elseif ($label -eq '边境腹地') { $tPct.Border } else { $tPct.Frontier }
    $newHtml = [regex]::Replace($newHtml, "$label \d+%&lt;/span&gt;", "$label $value%&lt;/span&gt;")
}
if ($newHtml -eq $html) {
    Write-Host '✓ 审阅地图数据已与设计源一致（幂等，无需写入）。'
    Write-Host "  文件:     $MapHtmlPath"
    Write-Host "  对象数:   $($rows.Count)（19 城镇 + 25 城堡 + 97 村庄）"
    Write-Host "  档位:     核心 0 / 过渡 20 / 腹地 30 / 前线 50"
    return
}
[System.IO.File]::WriteAllText($MapHtmlPath, $newHtml, (New-Object System.Text.UTF8Encoding($false)))

# 写后自检：重新提取并解码，确认数组可解析为 141 个对象。
$verifyText = Get-Content -LiteralPath $MapHtmlPath -Raw -Encoding UTF8
$verifyMatch = [regex]::Match($verifyText, $dataPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $verifyMatch.Success) {
    throw '写后自检失败：无法在生成的 HTML 中找到 const data 数组。'
}
$verifyRows = [System.Net.WebUtility]::HtmlDecode($verifyMatch.Groups[1].Value) | ConvertFrom-Json
$verifyCount = if ($null -eq $verifyRows) { 0 } else { $verifyRows.Count }
if ($verifyCount -ne 141) {
    throw "写后自检失败：解码后对象数为 $verifyCount，应为 141。"
}
$verifyDecoded = [System.Net.WebUtility]::HtmlDecode($verifyText)
$verifyLegend = "过渡圈 $($tPct.Transition)% · 边境腹地 $($tPct.Border)% · 外缘前线 $($tPct.Frontier)%"
if ($verifyDecoded -notmatch [regex]::Escape($verifyLegend)) {
    throw "写后自检失败：图例文字未同步为最新档位（期望 $verifyLegend）。"
}

Write-Host ''
Write-Host '✓ 审阅地图数据已由设计源同源重建：'
Write-Host "  文件:     $MapHtmlPath"
Write-Host "  对象数:   $($rows.Count)（19 城镇 + 25 城堡 + 97 村庄）"
Write-Host "  档位:     核心 0 / 过渡 20 / 腹地 30 / 前线 50"
