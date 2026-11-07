# 构建、部署与打包

本工程使用 .NET SDK `6.0.428`、`netstandard2.0` 与 x64 目标。`global.json` 禁止自动滚动到其他 SDK，以避免不同开发机生成不可复现的程序集。

## 前提

- 已安装 .NET SDK `6.0.428`，且 `dotnet --version` 输出 `6.0.428`。
- 已安装 Bannerlord `v1.4.8` 与 War Sails / NavalDLC `v1.2.8`。
- PowerShell 7 或 Windows PowerShell 5.1。

游戏安装路径不会写入工程。每次通过 `-BannerlordGameDir` 参数传入，或在当前终端设置 `BANNERLORD_GAME_DIR`：

```powershell
$gameDir = '<Bannerlord 根目录>'
```

## Release 构建

```powershell
.\scripts\build.ps1 -BannerlordGameDir $gameDir -Configuration Release
```

脚本会执行还原、编译、SDK 版本检查、产物检查和官方 DLL 泄漏检查。输出位于：

```text
artifacts/build/Release/WXProvincialFreeCompany.dll
```

游戏的 `TaleWorlds.*.dll` 只以 `Private=false` 方式参与编译，不会复制到输出目录。

## 部署到本机游戏

```powershell
.\scripts\deploy.ps1 -BannerlordGameDir $gameDir -Configuration Release
```

脚本会先构建，然后把 XML 数据和本 MOD 的 DLL 组合到：

```text
<BannerlordGameDir>/Modules/WXProvincialFreeCompany
```

DLL 默认同时部署到 `Win64_Shipping_Client` 与 `Win64_Shipping_wEditor`，以便游戏客户端和 Modding Kit 读取。若只部署客户端，可传入 `-IncludeEditor:$false`。

确认已完成相同配置的构建时，可以追加 `-SkipBuild`。

## 生成发布包

```powershell
.\scripts\package.ps1 -BannerlordGameDir $gameDir -Configuration Release
```

压缩包位于（文件名版本取自 `SubModule.xml`，随模块版本自动变化）：

```text
artifacts/package/WXProvincialFreeCompany-<版本号>.zip
```

脚本会检查压缩包内存在 `SubModule.xml` 与本 MOD 的 DLL，并拒绝包含任何 `TaleWorlds*.dll` 或 `NavalDLC*.dll` 的包。

## 地域征募数据层：设计源校验与运行时配置生成

地域征募的唯一机器可读设计源是 `design/imperial_settlements.tsv`（141 个帝国聚落对象）。审阅地图与运行时 XML 均由脚本同源生成，禁止手改设计源之外的产物。

校验设计源（字段规则 + 对照真实游戏 `settlements.xml` + 与审阅地图 HTML 的漂移检测）：

```powershell
.\scripts\validate-design.ps1 -BannerlordGameDir $gameDir
```

生成运行时配置 `src/WXProvincialFreeCompany/ModuleData/regional_recruitment.xml`（116 个征兵点 = 19 城镇 + 97 村庄）：

```powershell
.\scripts\generate-recruitment-xml.ps1 -BannerlordGameDir $gameDir
```

生成脚本会先执行设计源校验，拒绝从错误数据产出运行时配置；生成后自动重新解析自检。`regional_recruitment.xml` 由 C# 配置加载器直接读取，不在 `SubModule.xml` 注册 XmlNode。设计源任何修改后必须重跑校验与生成，并把两者一并提交。

## 纯逻辑冒烟测试

Release 构建完成后运行（不依赖游戏目录；若设置 `BANNERLORD_GAME_DIR` 还会追加命令特性注册检查）：

```powershell
dotnet run --project tests\SmokeTest -c Release
```

覆盖：116 个征兵点解析、四档概率一万次模拟（0% 严格 0 命中、其余 ±2pp 容差）、非法配置拒绝与 `wx_pfc.*` 命令特性注册。

## v0.5.6 手动加载与战役验证

### 验收边界

v0.5.6 已完成代码、XML、构建、打包与部署验证。下面的实机验收用于确认 Bannerlord 运行时实际加载了新 DLL、GameText、设置和 `VolunteerModel` 包装器。测试不要求兼容旧开发存档，推荐新建独立测试档；本轮不自动启动 Steam、启动器、游戏或 Modding Kit。

### A. 启动与界面（发布阻断项）

1. 完全退出此前运行的游戏和启动器，再重新启动，避免旧 GameText 留在进程内。
2. 在单人模组列表启用 War Sails / NavalDLC 与 `WX Provincial Free Company`，确认版本为 `v0.5.6`，没有 DLL、依赖或版本错误。
3. 分别确认沙盒和剧情战役至少能够进入世界地图；主菜单、自定义战斗和战役均无加载崩溃。
4. 打开“选项 → 战役选项”，确认 6 个地域征募滑块显示中文标题、数值和悬停说明，不再出现 `_campaign_options_type doesn't exist` 或 `str_campaign_options_description doesn't exist`。
5. 修改一项并应用，退出设置再进入；随后重启游戏，确认数值仍然保留。设置文件应位于：

```text
Documents\Mount and Blade II Bannerlord\Configs\WXProvincialFreeCompany.xml
```

### B. 配置诊断（发布阻断项）

游戏控制台字体不含中文字形，因此 `wx_pfc.*` 命令固定输出英文。若控制台不可用，可在测试档期间临时启用 Bannerlord `cheat_mode`，测试结束后恢复。

依次执行：

```text
wx_pfc.profile town_EN6
wx_pfc.profile town_ES2
wx_pfc.profile town_EN4
wx_pfc.profile town_EN3
wx_pfc.sample town_EN6 10000
```

预期：

| 聚落 | 设计档位 | 预期输出 |
|---|---|---|
| `town_EN6` 安普雷拉 | 东境前线 | `East/Frontier`、`designChance=0.50` |
| `town_ES2` 沃斯特鲁姆 | 南境腹地 | `South/Border`、`designChance=0.30` |
| `town_EN4` 阿耳戈隆 | 北境过渡区 | `North/Transition`、`designChance=0.20` |
| `town_EN3` 萨涅俄帕 | 帝国核心 | `Core/Core`、概率与上限均为 0 |

所有非核心输出中的 `effectiveChance` 和 `maxSlots` 必须与战役选项当前值一致；修改滑块后再次执行，应立即反映新值。`sample` 是固定种子模拟，不改动存档与战役随机数，其 `observed` 应接近当前 `effectiveChance`。

### C. 地域征募与槽位上限（发布阻断项）

为了缩短测试时间，可暂时把三档概率设为 100%，槽位上限设为 6。注意：设置只影响以后填充的空志愿兵槽，不会把已经存在的兵种瞬间改写。可以先从当地要人处招走现有志愿兵制造空槽，再等待游戏日刷新。

建议至少检查以下五处：

| 地点 | 应出现的结果 |
|---|---|
| 厄庇克洛忒亚或狄亚特马及其村庄 | 仅北境 T1 `北境边民` |
| 达努斯提卡及其村庄 | 仅南境 T1 `南境边民` |
| 拉革塔或贾尔马律斯及其村庄 | 仅西境 T1 `西境边民` |
| 安普雷拉及其村庄 | 仅东境 T1 `东境边民` |
| 萨涅俄帕、吕卡隆等核心区 | 不出现地域 T1，保持原版帝国新兵 |

随后把前线概率恢复 50%、腹地 30%、过渡区 20%，槽位恢复 3/2/1。逐个观察同一要人的六个槽位，地域兵数量不得超过该地点当前档位的上限。上限只阻止新的地域兵替换，不会删除已经生成的兵，也不会清洗玩家或领主队伍中的现有单位。

### D. 贵族兵、升级树与玩家路径（发布阻断项）

1. 在能提供帝国贵族兵的强力地主处观察刷新：`imperial_vigla_recruit` 及其后续贵族升级线不得被地域兵替换。
2. 招募四种地域 T1，各自确认只能升级到本地域 T2，再沿单线升级至 T5，不得串到其他地域。
3. 在自定义战斗中确认四条 T1–T5 仍能从帝国阵营选择；北境 T4/T5 仍识别为水兵，北境 T1–T3 仍为普通陆军。
4. 保存、退出并读取测试档，确认地域兵、升级关系、选项数值和征募机制继续有效。

### E. AI 与全局影响（阶段验收项）

在新档 Day 0、30、60、120 分别执行并保存输出：

```text
wx_pfc.audit_volunteers
wx_pfc.audit_parties
```

`audit_volunteers` 应只统计能够征兵的有效要人；`imperialNoble` 不应因地域替换机制消失。`audit_parties` 中的 `regional` 与 `partiesWithRegional` 应随时间逐步增加，证明 AI 领主确实从同一志愿兵池招募并升级地域兵，但帝国正规军仍应保持军队主体。

这组长期数据主要用于平衡 50/30/20 与 3/2/1，并非证明代码能否加载的前置条件。如果某个日期发现帝国领主普遍被单一地域兵淹没，应保存两个 audit 输出和存档，再调整默认档位，不要直接重写领主部队。

### 最终通过标准

- 无启动、进战役、保存或读取崩溃。
- 6 个滑块文本、说明、数值及持久化全部正确。
- `profile/sample` 使用实时设置；核心区恒为 0。
- 四地域只生成对应 T1，单名要人不突破当前槽位上限。
- 帝国贵族兵与非 `imperial_recruit` 结果保持原版。
- 玩家能完整升级四条路线，AI 能逐步招募它们，原版帝国正规军仍是长期主体。
