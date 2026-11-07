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

## v0.5.0 骨架的手动加载验证

1. 打开 Bannerlord 启动器，在单人模组列表启用 War Sails / NavalDLC 与 `WX Provincial Free Company`。
2. 确认启动器没有报告 DLL 缺失、依赖缺失或版本格式错误。
3. 启动游戏，进入主菜单，检查未发生加载崩溃。
4. 进入自定义战斗，确认 v0.4.5 的四条地域兵种仍可选择。
5. 新建沙盒或战役，进入世界地图并保存、读取一次，确认无异常。
6. 本骨架不会改变志愿兵池；若地域兵仍无法从普通要人处招募，这是当前里程碑的预期结果。

本轮不自动启动 Steam、启动器、游戏或 Modding Kit，最后的运行时加载验证由用户手动完成。
