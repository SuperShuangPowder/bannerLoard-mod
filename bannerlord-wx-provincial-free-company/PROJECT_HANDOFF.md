# WXProvincialFreeCompany 项目交接说明

> 本文用于让第一次接触项目的开发者或Agent在较短时间内建立正确认知。它描述当前稳定状态和工作边界，不替代README、BUILDING或完整开发日志。

## 一分钟结论

- 当前稳定版本：`v0.5.6-campaign-option-texts`。
- 目标游戏：Bannerlord `v1.4.8`；必需DLC：War Sails / NavalDLC `v1.2.8`。
- 第一阶段已经完成：四套地域T1–T5兵种、全图地域征募、动态概率与槽位上限、游戏内设置、诊断命令、构建部署链和关键实机验收均已落地。
- 当前没有发布阻断级代码缺口。下一版本应从用户新档长期游玩的真实反馈中产生。
- 不要自动启动Steam、游戏、启动器或Modding Kit；运行时测试由用户手动完成。

## MOD现在做什么

MOD为帝国增加四条地域兵种树：

```text
北境：边民 → 行省征召兵 → 行省辅兵 → 自由军盾斧兵 → 自由军盾卫
南境：边民 → 行省征召兵 → 行省掷矛兵 → 自由军突击兵 → 自由军破阵卫
西境：边民 → 行省征召兵 → 行省弩手 → 自由军重弩手 → 自由军破甲弩卫
东境：边民 → 行省征召兵 → 行省骑射手 → 自由军游骑射手 → 自由军游骑卫
```

战役中，116个帝国城镇/村庄征兵点按照统一帝国地理划分为北、南、西、东和核心区。当原版当前有效 `VolunteerModel` 正好生成 `imperial_recruit` 时，MOD才可能按地域、概率和单要人槽位上限将其替换为对应地域T1。

默认值：

| 档位 | 替换概率 | 单要人地域兵槽位上限 |
|---|---:|---:|
| 外缘前线 | 50% | 3 |
| 边境腹地 | 30% | 2 |
| 过渡区 | 20% | 1 |
| 帝国核心 | 0% | 0 |

玩家可以在“选项 → 战役选项”中实时调整六个参数。设置保存在Bannerlord用户配置目录的 `WXProvincialFreeCompany.xml` 中。

## 必须保留的行为边界

- 只替换精确的 `imperial_recruit`。
- `imperial_vigla_recruit`及其整条帝国贵族升级线受到保护。
- 非帝国新兵、其他MOD模型已经返回的其他兵种、核心区结果均保持原样。
- 配置加载失败或包装器异常时回退原版结果，不能让增强功能破坏战役。
- AI领主和玩家使用同一志愿兵池；MOD不直接重写领主模板或扫描、篡改现有军队。
- 第一版不区分北境沿海和内陆；北境T4/T5才具有专业水兵身份。
- v0.4.2以前旧共同T2不再兼容，推荐新档。

## 架构地图

```text
design/imperial_settlements.tsv（唯一设计源，141个聚落对象）
        │
        ├─ scripts/validate-design.ps1
        ├─ scripts/generate-recruitment-xml.ps1
        │        └─ ModuleData/regional_recruitment.xml（116个征兵点）
        └─ scripts/generate-map.ps1

ModuleData XML/XSLT                C#战役模块
兵种、装备、升级、本地化          配置、设置、诊断、VolunteerModel装饰器
        │                              │
        └──────── build/deploy/package ─┘
                         │
             完整模块 = 静态src + 自有DLL
```

关键C#阅读顺序：

1. `RegionalVolunteerDecision.cs`：纯替换规则。
2. `RegionalRecruitmentProfile.cs` 与 `RegionalRecruitmentRegistry.cs`：配置领域模型与解析。
3. `RegionalRecruitmentSettings.cs` 与 `WxPfcCampaignOptionsProvider.cs`：设置、持久化与UI。
4. `RegionalVolunteerModel.cs`：官方模型装饰器。
5. `SubModule.cs`：生命周期和模型注册。
6. `RegionalRecruitmentDebugCommands.cs`：只读诊断命令。
7. `tests/SmokeTest/Program.cs`：当前自动化契约。

## 目录职责

| 路径 | 职责 | 是否人工直接修改 |
|---|---|---|
| `code/` | C#源码 | 是 |
| `design/imperial_settlements.tsv` | 地域征募唯一设计源 | 是 |
| `src/` | 静态Bannerlord模块源 | 是，但不包含最终DLL |
| `src/.../regional_recruitment.xml` | 运行时地域配置 | 否，由脚本生成 |
| `scripts/` | 校验、生成、构建、部署和打包 | 是 |
| `tests/SmokeTest/` | 纯逻辑和结构冒烟测试 | 是 |
| `artifacts/` | 本机构建、暂存、压缩包 | 否，不提交Git |
| `docs/` | 当前开发日志与补充教程 | 是 |

## 构建与验证

完整命令和手动验收表以 [BUILDING.md](./BUILDING.md) 为准。最短路径如下：

```powershell
$gameDir = '<Bannerlord 根目录>'
./scripts/validate-design.ps1 -BannerlordGameDir $gameDir
./scripts/build.ps1 -BannerlordGameDir $gameDir -Configuration Release
dotnet run --project tests/SmokeTest -c Release
./scripts/deploy.ps1 -BannerlordGameDir $gameDir -Configuration Release
./scripts/package.ps1 -BannerlordGameDir $gameDir -Configuration Release
```

部署和打包脚本会组合静态源与编译后的 `WXProvincialFreeCompany.dll`，并拒绝官方DLL进入产物。不得把 `src` 目录单独当成完整可安装MOD。

游戏内只读诊断命令：

```text
wx_pfc.profile <SettlementId>
wx_pfc.sample <SettlementId> <Count>
wx_pfc.audit_volunteers
wx_pfc.audit_parties
```

## 文本与本地化注意事项

战役选项文本有两层：

1. `ModuleData/wx_campaign_strings.xml` 注册 `GameText` 及variation；
2. `Languages/en` 与 `Languages/CNs` 提供翻译。

语言文件不能单独创建GameText。若再次出现 `_campaign_options_type doesn't exist` 或 `str_campaign_options_description doesn't exist`，优先检查GameText注册和 `SubModule.xml` 加载，而不是继续添加伪语言键。

## 文档事实优先级

发生冲突时按以下顺序判断：

1. 当前源码、XML、项目文件与脚本；
2. 模块 `README.md`、本交接说明和 `BUILDING.md`；
3. `src/WXProvincialFreeCompany/CHANGELOG.md`；
4. 两册当前开发日志；
5. 归档日志和早期设计讨论。

当前两册日志：

- [第1册：战斗设计部分](./docs/骑砍2兵种扩展MOD开发日志-1战斗设计部分.md)
- [第2册：战役设计部分](./docs/骑砍2兵种扩展MOD开发日志-2战役设计部分.md)
- [帝国地域征募分区地图](./docs/帝国地域征募分区地图-世界观校准版.html)

## 尚未完成但不阻断第一阶段的事项

- 在正常新档中积累Day 0/30/60/120志愿兵池和AI军队统计。
- 根据长期体验评估50/30/20与3/2/1默认值。
- 建立与其他 `VolunteerModel`、帝国兵种XML或相同XSLT节点MOD的兼容说明。
- 完成创意工坊私密上传、订阅版与本地版隔离测试。
- 选择开源许可证。
- 为不依赖官方游戏程序集的检查增加GitHub Actions。
- 游戏或War Sails升级后重新编译并完成兼容验收。

这些事项应按真实需求推进，不应被误判为当前版本无法游玩的阻断错误。

## 接手后的第一步

开始任何新需求前：

1. 执行 `git status`，保留用户已有改动。
2. 确认用户希望“解释、诊断”还是“实际修改”。
3. 明确本轮版本边界、完成定义和人工验收项。
4. 查阅相关开发日志，但以当前源码为准。
5. 使用独立分支开发；除非用户明确要求，不自动推送、建PR或合并。

如果没有新的实机反馈，正确行动通常是维持 `v0.5.6` 稳定基线，而不是主动扩大功能范围。
