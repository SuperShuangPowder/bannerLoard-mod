# Agent 工作约定

本文件适用于整个 `bannerLoard-mod` 仓库。任何新 Agent 在修改代码或文档前，都应先完成下面的阅读与检查。

## 必读顺序

1. 仓库根目录 `README.md`。
2. `bannerlord-wx-provincial-free-company/PROJECT_HANDOFF.md`。
3. `bannerlord-wx-provincial-free-company/README.md`。
4. `bannerlord-wx-provincial-free-company/BUILDING.md`。
5. 与任务相关的源码、脚本和两册开发日志；不要默认从头通读全部历史。

## 当前稳定基线

- MOD：`WXProvincialFreeCompany v0.5.6-campaign-option-texts`。
- 游戏：Bannerlord `v1.4.8`。
- DLC：War Sails / NavalDLC `v1.2.8`，必需依赖。
- SDK：.NET SDK `6.0.428`，由项目 `global.json` 精确锁定。
- C#目标：`netstandard2.0`、x64、0警告、0错误。
- Git稳定基线：`main`；第一阶段关键验收已通过，目前没有必须立即实现的新功能。

## 用户授权与安全边界

- 未经用户在当前任务中明确授权，不得自动启动Steam、Bannerlord、启动器、Modding Kit或编辑器，也不得代替用户操作这些界面。
- 不得修改 `Native`、`SandBoxCore`、`Sandbox`、`StoryMode`、`CustomBattle`、`NavalDLC` 等官方模块文件。
- 游戏目录只允许通过项目部署脚本写入本MOD自己的 `Modules/WXProvincialFreeCompany` 目录；部署前应向用户说明。
- 不得把 `TaleWorlds*.dll`、`NavalDLC*.dll`、其他官方资产、存档、日志、崩溃转储、令牌或本机账号信息提交进仓库。
- 不得在工程中写死本机游戏安装路径；使用 `-BannerlordGameDir` 或 `BANNERLORD_GAME_DIR`。
- 现有工作树中的用户改动必须保留；不要使用破坏性Git命令清理工作区。
- 推送、创建PR和合并必须符合用户当次授权；本地修改请求不自动包含远端发布授权。

## 唯一真相源与生成物

- 地域聚落设计的唯一人工维护源是 `design/imperial_settlements.tsv`。
- `src/WXProvincialFreeCompany/ModuleData/regional_recruitment.xml` 由 `scripts/generate-recruitment-xml.ps1` 生成，不应脱离TSV手改。
- 地域审阅地图由 `scripts/generate-map.ps1` 同源生成。
- `src/WXProvincialFreeCompany` 只包含静态模块源；完整可安装模块还必须组合编译后的 `WXProvincialFreeCompany.dll`。
- `artifacts/` 是构建、暂存和打包产物，不提交Git。
- 项目README、BUILDING、CHANGELOG、两册开发日志和交接说明必须与版本行为同步更新。

## 架构边界

- 固定兵种、装备、升级树和本地化优先使用XML。
- 精确变换原版XML节点时使用XSLT；修改前必须确认是追加、合并还是替换语义。
- 动态战役征募使用官方 `VolunteerModel` 装饰器，不默认引入Harmony。
- 只在原模型结果精确为 `imperial_recruit` 时考虑地域T1替换；帝国贵族兵和其他结果必须原样保留。
- 配置缺失或运行时异常时必须安全回退原版结果，并保留可诊断日志。
- 概率控制新槽产生速度，单要人槽位上限控制状态规模；不要混淆两种约束。

## 修改与验证要求

在项目目录执行，游戏路径通过参数传入：

```powershell
$gameDir = '<Bannerlord 根目录>'
```

### 仅文档修改

- 检查相对链接、版本号、文件名和中文编码。
- 确认没有把旧日志重新描述成当前实现。

### TSV、XML、XSLT或本地化修改

- 执行设计源校验和必要的生成脚本。
- 运行 `dotnet run --project tests\SmokeTest -c Release`。
- 检查模块版本、ID、升级引用和中英文键。
- 涉及外观、AI换武器或自定义战斗分类时，必须列出人工验收项。

### C#战役逻辑修改

- 执行 `scripts/build.ps1`，保持0警告、0错误。
- 运行SmokeTest。
- 确认构建与压缩包没有官方DLL。
- 说明对贵族兵、核心区、AI领主、存档和其他 `VolunteerModel` MOD的影响。
- 自动测试通过后仍需用户亲自启动游戏完成运行时验收。

### 部署与发布

- 使用 `scripts/deploy.ps1`，不要依靠手工复制DLL。
- 使用 `scripts/package.ps1` 生成完整发布包。
- 不自动启动游戏验证；向用户提供清晰的手动验收步骤。
- 只有用户明确要求时才推送远端、创建PR或合并。

## 文档阅读边界

- `docs/骑砍2兵种扩展MOD开发日志-1战斗设计部分.md` 是当前战斗与兵种历史。
- `docs/骑砍2兵种扩展MOD开发日志-2战役设计部分.md` 是当前战役系统历史。
- `docs/骑砍2兵种拓展MOD开发日志.md` 只是旧混合日志的归档入口，不是当前事实源。
- 历史记录用于解释决策演进；当前行为以模块README、PROJECT_HANDOFF、BUILDING、CHANGELOG和实际源码为准。

## 完成定义

不要仅以“代码已经写完”宣告完成。交付至少应说明：

1. 修改范围与明确未修改的边界；
2. 自动验证结果；
3. 是否已部署、打包或推送；
4. 仍需用户执行的人工验收；
5. 文档和CHANGELOG是否同步；
6. 回滚点或对应Git提交。
