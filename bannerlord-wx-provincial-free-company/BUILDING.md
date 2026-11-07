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

压缩包位于：

```text
artifacts/package/WXProvincialFreeCompany-v0.5.0.zip
```

脚本会检查压缩包内存在 `SubModule.xml` 与本 MOD 的 DLL，并拒绝包含任何 `TaleWorlds*.dll` 或 `NavalDLC*.dll` 的包。

## v0.5.0 骨架的手动加载验证

1. 打开 Bannerlord 启动器，在单人模组列表启用 War Sails / NavalDLC 与 `WX Provincial Free Company`。
2. 确认启动器没有报告 DLL 缺失、依赖缺失或版本格式错误。
3. 启动游戏，进入主菜单，检查未发生加载崩溃。
4. 进入自定义战斗，确认 v0.4.5 的四条地域兵种仍可选择。
5. 新建沙盒或战役，进入世界地图并保存、读取一次，确认无异常。
6. 本骨架不会改变志愿兵池；若地域兵仍无法从普通要人处招募，这是当前里程碑的预期结果。

本轮不自动启动 Steam、启动器、游戏或 Modding Kit，最后的运行时加载验证由用户手动完成。
