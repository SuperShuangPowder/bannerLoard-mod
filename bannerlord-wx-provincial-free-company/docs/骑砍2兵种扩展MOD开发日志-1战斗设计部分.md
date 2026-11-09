# 骑砍2兵种扩展 MOD 开发日志——第1册：战斗设计部分

> - **项目代号：** `WXProvincialFreeCompany`
> - **项目名称：** 帝国行省自由军
> - **文档用途：** 记录开发环境、兵种树、技能装备、视觉设计、战斗平衡、XML/XSLT、部署与发布历史
> - **建档日期：** 2026-10-31
> - **当前版本：** `v0.5.6-campaign-option-texts`（兵种与战斗设计阶段已完成，战役关键验收已通过）
> - **战役设计日志：** [`骑砍2兵种扩展MOD开发日志-2战役设计部分.md`](./骑砍2兵种扩展MOD开发日志-2战役设计部分.md)

---

## 阅读导航

> **推荐读法：** 初次了解项目时先看“阶段收尾”，再按需要回查对应版本；研究完整设计演进时，再从战斗记录001顺序阅读。

| 阅读阶段 | 入口 | 主要内容 |
|---|---|---|
| 环境与工具 | [战斗记录001–005](#combat-environment) | 游戏版本、模块、Modding Kit、最小加载测试 |
| 原型与版本库 | [战斗记录006–008](#combat-prototype) | T2–T5 原型、中文、本地与 GitHub 仓库 |
| 四地域结构 | [战斗记录009–011](#combat-regional-structure) | 四分支构想、v0.3.0 骨架、首次 PR |
| 分支深化与平衡 | [战斗记录012–019](#combat-branch-development) | 北、南、西、东路线、XSLT 与 v0.3.8 平衡 |
| 战役内容迁移 | [迁移说明](#combat-campaign-migration) | 原全局记录020、021的迁移位置与分册边界 |
| 地域低阶兵 | [战斗记录020–021](#combat-low-tier) | v0.4.0 独立 T1/T2 与 v0.4.1 视觉重构 |
| 实测修正 | [战斗记录022](#combat-record-026) | v0.4.2 旧公共树清理、东境 T1、南境 T2 与西境 T5 修正 |
| 海战专精与低阶平衡 | [战斗记录023](#combat-record-028) | v0.4.3 粗制猎弓与北境全线水兵化 |
| 水兵门槛修正 | [战斗记录024](#combat-record-030) | v0.4.4 将北境专业水兵收紧至 T4–T5 |
| 自定义战斗水兵标识 | [战斗记录025](#combat-record-031) | v0.4.5 修复北境 T4/T5 在自定义战斗中的兵种类型显示 |
| 阶段收尾 | [里程碑与后续](#combat-next) | v0.5.6 验收结论、历史待办归档与未来战斗层候选项 |

### 编号规则

本册现在使用连续的“战斗记录”编号，阅读顺序为001–025。为了不破坏旧讨论、Git提交和第二册中的引用，拆册前的全局记录号仍保留在后六条标题中。

<details>
<summary>展开本册编号与原全局记录号对照</summary>

| 本册编号 | 原全局编号 | 对应版本 |
|---|---|---|
| 战斗记录020 | 记录022 | v0.4.0 |
| 战斗记录021 | 记录023 | v0.4.1 |
| 战斗记录022 | 记录026 | v0.4.2 |
| 战斗记录023 | 记录028 | v0.4.3 |
| 战斗记录024 | 记录030 | v0.4.4 |
| 战斗记录025 | 记录031 | v0.4.5 |

</details>

---

## 项目目标

制作一个范围可控、能够完整发布的《骑马与砍杀2：霸主》兵种拓展 MOD，以此掌握 Bannerlord 现代版本的模块规范、XML 数据结构、合并机制、本地化、测试与发布流程。

首版目标暂定为一条从帝国新兵分出的行省自由军支线：

```text
T1 帝国新兵（原版）
  └─ T2 行省征召兵
       └─ T3 行省辅兵
            └─ T4 自由军团战士
                 └─ T5 自由军团老兵
```

这条路线不代表帝国中央正规军，而是忠于地方、雇主和军饷的行省武装。帝国阵营可将其视为地方辅兵，非帝国阵营可将其视为投效者、雇佣军或占领区地方兵，因此玩家与 NPC 使用它都具有叙事合理性。

v0.1 只使用原版模型、装备和动画，以 XML 数据开发为主，不引入 Harmony、ButterLib、UIExtenderEx、MCM、自定义 DLL、新模型或新场景。v0.1 接受该路线作为全局兵种树的一部分，不实现玩家专属或阵营条件升级。

---

<a id="combat-environment"></a>

## 战斗记录 001：开发环境准备与校验

**记录日期：** 2026-10-31  
**校验方式：** 只读检查游戏目录、Steam 清单、官方模块清单、LauncherData 与 Workshop 模块清单。  
**校验结论：** 游戏与 War Sails 已达到选定开发基线；当前模块无重复 ID，但基础 Modding Kit 编辑器尚未安装完整。

### 1. 游戏安装位置

```text
C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord
```

目录存在，客户端程序与官方模块均可读取。

### 2. 当前版本

| 项目 | 已确认版本 |
|---|---|
| Bannerlord | `v1.4.8.119303` |
| Steam Build ID | `24573425` |
| War Sails / NavalDLC | `v1.2.8.119303` |
| Steam 清单最后更新时间 | `2026-10-31 17:46:03 +08:00` |

这与项目此前选定的稳定开发基线一致：

```text
开发基线：Bannerlord v1.4.8
可选 DLC：War Sails v1.2.8
Beta 版本：暂不采用
```

### 3. 官方与本地游戏模块

游戏 `Modules` 目录当前包含：

```text
Native
SandBoxCore
SandBox
StoryMode
CustomBattle
BirthAndDeath
Multiplayer
NavalDLC
FastMode
```

其中 Native、SandBoxCore、Sandbox、StoryMode、CustomBattle、BirthAndDeath 均为 `v1.4.8`，NavalDLC 为 `v1.2.8`。

### 4. 启动器当前启用顺序

| 顺序 | 模块 ID | 版本 | 来源 |
|---:|---|---|---|
| 1 | `Bannerlord.Harmony` | `v2.4.2.248` | Workshop |
| 2 | `Bannerlord.ButterLib` | `v2.12.0` | Workshop |
| 3 | `Bannerlord.UIExtenderEx` | `v3.0.3` | Workshop |
| 4 | `Bannerlord.MBOptionScreen` | `v5.12.3` | Workshop |
| 5 | `Native` | `v1.4.8` | 游戏目录 |
| 6 | `SandBoxCore` | `v1.4.8` | 游戏目录 |
| 7 | `Sandbox` | `v1.4.8` | 游戏目录 |
| 8 | `StoryMode` | `v1.4.8` | 游戏目录 |
| 9 | `BirthAndDeath` | `v1.4.8` | 游戏目录 |
| 10 | `CustomBattle` | `v1.4.8` | 游戏目录 |
| 11 | `HealOnKillUpdated` | `v1.1.0.8` | Workshop |
| 12 | `both_perks` | `v1.0.0` | Workshop |
| 13 | `NavalDLC` | `v1.2.8` | 游戏目录 |
| 14 | `FastMode` | `v1.4.8` | 游戏目录 |
| 15 | `EnableAchievement` | `v0.0.2` | Workshop |
| 16 | `UnlimitedSmithingStamina` | `v1.0.0` | Workshop |

### 5. 当前环境完整性检查

- 当前实际加载的 Harmony 为 `v2.4.2.248`。
- UIExtenderEx 为 `v3.0.3`，ButterLib 为 `v2.12.0`，MCM 为 `v5.12.3`。
- 未发现重复模块 ID。
- Workshop 中未发现额外的“已安装但未启用”模块残留。

### 6. 当前第三方 MOD 风险

以下模块并未明确以 Bannerlord 1.4.8 为依赖基线：

| 模块 | 清单声明 | 判断 |
|---|---|---|
| `HealOnKillUpdated` | 依赖官方模块 `v1.3.13` | 可能向上兼容，但开发排错时应关闭 |
| `UnlimitedSmithingStamina` | 依赖官方模块 `v1.3.15` | 可能向上兼容，但开发排错时应关闭 |
| `both_perks` | 未声明精确游戏版本 | 兼容性未知，开发排错时应关闭 |
| `EnableAchievement` | 只声明 Harmony | 与兵种 MOD 无关，开发排错时可关闭 |

上述信息不等同于这些 MOD 已确认存在错误，只表示它们会增加测试变量。

### 7. Modding Kit 状态

Steam 游戏清单已经挂载：

```text
War Sails DLC depot：2927200
War Sails Modding Kit depot：4456490
```

但当前没有检测到：

```text
基础 Modding Kit 应用清单：appmanifest_1393600.acf
bin\Win64_Shipping_wEditor
XMLEditor
Tools
```

因此可判断：War Sails 相关资源已经随 Steam 清单下载，但基础 Bannerlord Modding Kit 编辑器尚未安装完整，当前不能直接启动官方编辑器。

后续需要从 Steam“工具”分类安装：

```text
Mount & Blade II: Bannerlord - Modding Kit
```

并确认 War Sails Modding Kit DLC 已启用。完成后应至少出现：

```text
bin\Win64_Shipping_wEditor
```

### 8. 正式开发使用的最小加载环境

v0.1 开发和测试时暂时关闭所有非必要第三方 MOD。推荐基线：

```text
Native
SandBoxCore
Sandbox
StoryMode
BirthAndDeath
CustomBattle
NavalDLC              # 只用于兼容性测试，可选
WXProvincialFreeCompany  # 本项目
```

Harmony、ButterLib、UIExtenderEx、MCM 在 v0.1 中没有直接用途，也应先关闭。这样出现模块加载、XML 合并、兵种装备或升级树问题时，排查范围最小。

### 9. 本次校验的阶段结论

| 验收项 | 状态 |
|---|---|
| Bannerlord 已切换到 `v1.4.8` | 通过 |
| War Sails 已安装并匹配 `v1.2.8` | 通过 |
| 新版 Harmony 四件套无重复 ID | 通过 |
| 已识别非必要 MOD 与版本风险 | 通过 |
| 基础 Modding Kit 编辑器可启动 | **未通过：尚未完整安装** |
| 可以开始纯 XML 文件勘察 | 通过 |
| 可以开始场景、资产或编辑器工作 | **暂缓** |

总体状态：**有条件通过。** 可以开始读取 1.4.8 原版 XML、确定兵种结构与建立模块目录；在需要官方 XML Editor、资源、场景或发布工具前，必须补齐 Modding Kit。

---

## 战斗记录 002：兵种主题调整与共享升级树决策

**记录日期：** 2026-10-31  
**目标：** 让新增兵种既适合非帝国玩家使用，也能在帝国 NPC 军队中自然出现。  
**结论：** 放弃“帝国边境守备队”旧方案，改为“帝国行省自由军”。旧方案尚未进入正式实现，不保留兼容包袱。

### 1. 调整原因

玩家下一档计划游玩诺德等非帝国阵营，并可能在帝国领土附近征兵。如果新兵最终全部成为帝国中央正规军，会削弱异国军队的代入感；但若把“帝国变节者”直接挂入公共升级树，帝国 NPC 领主也可能批量升级出变节者，同样不自然。

Bannerlord 的兵种定义是全局共享数据。只要在 `imperial_recruit` 的 `upgrade_targets` 中追加新目标，所有持有帝国新兵的合格队伍都能读取它。NPC 队伍会在每日结算和战斗结束后，根据兵种经验、金币、工资限制以及领主的兵种偏好，从全部可行目标中自动选择路线。

因此，本项目不把 NPC 使用视为需要隐藏的副作用，而是把它纳入世界观设计。

### 2. 新兵种线

| 阶级 | 中文名 | 英文工作名 | 战场职责 |
|---|---|---|---|
| T2 | 行省征召兵 | Provincial Levy | 持盾短矛的地方武装 |
| T3 | 行省辅兵 | Provincial Auxiliary | 长矛、盾牌与少量投枪 |
| T4 | 自由军团战士 | Free Company Warrior | 持盾长矛／战斧混合步兵 |
| T5 | 自由军团老兵 | Free Company Veteran | 机动型精锐反骑掠袭步兵 |

拟定对象 ID：

```text
wx_provincial_levy
wx_provincial_auxiliary
wx_free_company_warrior
wx_free_company_veteran
```

### 3. 世界观解释

- 帝国领主使用：地方征召兵、行省辅兵或受雇的自由军团。
- 非帝国领主使用：投效者、地方向导、占领区士兵或帝国出身的佣兵。
- 士兵忠诚对象主要是家乡、雇主与军饷，而不是某一位帝国皇帝。
- 帝国内战和边疆失序使这种忠诚模糊的地方武装具备合理生存空间。

### 4. 战斗定位与边界

- 核心武器组合：长矛、单手斧或剑、盾牌、投枪。
- 护甲与正面持久力低于帝国军团步兵。
- 生存能力高于无盾的帝国双刃枪兵。
- 跑动、投掷和反骑能力较强，适合陆战、追击和劫掠。
- 不使用 `NavalSoldier` 特性，不与 War Sails 帝国水手线争夺海战定位。
- 核心版本暂不引用 War Sails 专属装备，保持 DLC 为可选兼容环境。

### 5. v0.1实现决策

- 从 `imperial_recruit` 追加 T2“行省征召兵”，保留原有帝国步兵和帝国弓箭手分支。
- 完整实现 T2 → T3 → T4 → T5 单线升级，不在首版继续分叉。
- 测试玩家手动升级与 NPC 日结／战后自动升级，记录各路线出现比例。
- 若后续确实需要“仅玩家”或“仅非帝国阵营”条件，再在 v0.2 评估小型 C# 行为；v0.1 不提前增加该复杂度。

---

## 战斗记录 003：最小模块加载测试计划

**记录日期：** 2026-10-31  
**目标：** 在编写任何正式兵种数据前，先证明游戏能够识别、加载和安全关闭本项目模块。  
**当前状态：** 已通过；2026-11-01 由用户手动完成全部界面测试。  
**测试原则：** 本轮不加入兵种、不修改升级树、不调整装备，只验证模块骨架与四种游戏类型的XML注册。

### 1. 为什么先做空模块测试

如果一开始同时加入四个兵种、装备、本地化和升级树，发生崩溃时无法迅速判断问题来自模块清单、XML注册、对象结构、物品ID还是合并规则。最小模块测试把变量压缩到两项：`SubModule.xml` 是否正确，以及空的 `NPCCharacters` 文件能否被游戏加载。

只有空模块通过，才进入 T2“行省征召兵”的单兵种原型。

### 2. 拟冻结的模块身份

```text
目录名：WXProvincialFreeCompany
模块 ID：WXProvincialFreeCompany
显示名：WX Provincial Free Company
版本号：v0.1.0
```

正式创建前再做一次拼写确认；一旦开始制作测试存档，不随意修改模块ID和对象ID。

### 3. 最小目录结构

```text
Mount & Blade II Bannerlord\
└─ Modules\
   └─ WXProvincialFreeCompany\
      ├─ SubModule.xml
      ├─ ModuleData\
      │  └─ spnpccharacters.xml
      ├─ README.md
      └─ CHANGELOG.md
```

本轮不创建 DLL、`bin`、Harmony补丁、模型、贴图、场景或正式语言文件。

### 4. 空兵种文件

`ModuleData\spnpccharacters.xml` 只保留合法根节点：

```xml
<?xml version="1.0" encoding="utf-8"?>
<NPCCharacters />
```

它不应包含任何 `NPCCharacter`，也不应修改原版对象。

### 5. SubModule.xml的关键要求

模块需要把 `spnpccharacters.xml` 注册为 `NPCCharacters`，并覆盖后续会使用的四种游戏类型：

```xml
<XmlNode>
  <XmlName id="NPCCharacters" path="spnpccharacters" />
  <IncludedGameTypes>
    <GameType value="Campaign" />
    <GameType value="CampaignStoryMode" />
    <GameType value="CustomGame" />
    <GameType value="EditorGame" />
  </IncludedGameTypes>
</XmlNode>
```

- `Campaign`：沙盒战役。
- `CampaignStoryMode`：剧情战役。
- `CustomGame`：主界面的自定义战斗。
- `EditorGame`：官方编辑器环境。

缺少 `CustomGame` 时，即使兵种能在战役中工作，也不会被自定义战斗加载。

### 6. 测试前加载环境

关闭所有非必要第三方MOD。首次测试只保留官方模块、本项目，以及进入对应模式所需的官方DLC：

```text
Native
SandBoxCore
Sandbox
StoryMode
BirthAndDeath
CustomBattle
NavalDLC
WXProvincialFreeCompany
```

本项目排在依赖的官方模块之后。首次测试不启用 Harmony、ButterLib、UIExtenderEx、MCM和其他玩法MOD。

### 7. 执行步骤

1. 备份新建模块目录；确认没有修改任何官方模块文件。
2. 启动官方Launcher，确认 `WX Provincial Free Company` 出现在Mods页面。
3. 检查模块版本、依赖和加载顺序，不带红色警告继续。
4. 只启用最小加载环境，启动游戏并进入主菜单。
5. 打开自定义战斗设置界面，确认能够正常选择阵营与地图；空模块不应改变任何兵种列表。
6. 返回主菜单并退出游戏，检查本次运行日志中是否存在本模块的XML解析或对象注册异常。
7. 再次启动Launcher并关闭本模块，确认游戏仍能正常进入主菜单。
8. 保存Launcher截图、主菜单截图、测试时间和日志路径，写回本记录的“结果”区域。

### 8. 通过标准

| 验收项 | 通过标准 |
|---|---|
| 模块识别 | Launcher显示正确名称、ID和版本 |
| 依赖检查 | 无缺失依赖、循环依赖或版本红色警告 |
| XML加载 | 空 `NPCCharacters` 文件不产生解析异常 |
| 主菜单 | 能稳定进入并正常退出 |
| 自定义战斗 | 能进入设置界面，不崩溃且原版兵种列表不变 |
| 可逆性 | 关闭本模块后游戏恢复原样 |
| 原版安全 | Native、SandBoxCore、Sandbox、StoryMode、CustomBattle和NavalDLC均未被修改 |

全部通过后，模块骨架标记为 `v0.1.0-skeleton`，进入下一里程碑：只添加 T2“行省征召兵”，先验证自定义战斗显示，再验证 `imperial_recruit` 的第三条T2升级路线。

### 9. 失败时的排查顺序

1. 模块目录是否多嵌套了一层。
2. 文件名是否精确为 `SubModule.xml`。
3. 模块ID、依赖ID和版本格式是否正确。
4. XML标签、引号和编码是否有效。
5. `XmlName` 的 `path` 是否与 `ModuleData\spnpccharacters.xml` 文件名一致。
6. `IncludedGameTypes` 是否包含正在测试的模式。
7. Launcher是否缓存了旧清单；完全退出后重新打开。
8. 日志中出现的第一个异常是什么；优先处理首个错误，不追逐后续连锁异常。

### 10. 实际测试结果

**测试日期：** 2026-11-01  
**执行方式：** 用户亲自操作 Bannerlord 启动器与游戏界面；Codex 不操作 Steam、Modding Kit、启动器或游戏界面。  
**用户报告：** 原始骨架未做任何手工文件修改，所有测试项均顺利通过。

| 验收项 | 结果 |
|---|---|
| Launcher 识别模块 | 通过 |
| 依赖与加载顺序 | 通过 |
| 进入主菜单 | 通过 |
| 进入自定义战斗 | 通过 |
| 关闭模块后的可逆性 | 通过 |
| 是否需要修正骨架文件 | 不需要 |

阶段结论：`v0.1.0-skeleton` 正式通过，可以进入 T2 单兵种原型。

---

## 战斗记录 004：通过Steam安装Modding Kit与搜索不到工具的排查

**记录日期：** 2026-10-31  
**目标：** 使用Steam官方渠道安装与 Bannerlord v1.4.8 匹配的开发工具，并在工具未出现在库中时按安全顺序排查。  
**当前状态：** 已安装并完成复检；详见记录 005。  
**安全原则：** 只使用Steam官方分发，不从论坛网盘或第三方站点下载所谓“完整工具包”。

### 1. 安装前准备

1. 完全退出 Bannerlord 游戏和Bannerlord启动器，Steam客户端保持运行。
2. 确认游戏本体仍为 `v1.4.8.119303`，不要在安装过程中切换游戏测试分支。
3. 工具包会使用游戏本体文件，必须与游戏保持相同版本并安装在同一块硬盘。当前游戏位于C盘Steam库，因此工具也选择同一个C盘库。
4. 暂时关闭Harmony四件套和其他第三方DLL模块，避免它们缺少 `Win64_Shipping_wEditor` 版本而干扰编辑器启动。

### 2. 在Steam库中找到工具

1. 打开Steam并进入“库”。
2. 打开左侧列表上方的类型筛选器；中文界面可能显示为“游戏与软件”。
3. 勾选“工具”，同时取消“仅显示已安装”“可立即游玩”或动态收藏等限制。
4. 在搜索框分别尝试：

```text
Bannerlord
Modding Kit
Modding Tools
```

工具可能显示为：

```text
Mount & Blade II: Bannerlord - Modding Kit
```

官方旧文档也称其为：

```text
Mount & Blade Bannerlord Modding Tools
```

不要误装 `Mount & Blade II: Dedicated Server`，它是多人服务器工具。

### 3. 正常安装步骤

1. 右键Modding Kit并选择“安装”。
2. 安装位置选择与Bannerlord相同的C盘Steam库：

```text
C:\Program Files (x86)\Steam\steamapps\common\
```

3. 等待Steam完成下载和校验。
4. 打开工具属性，确认下载未暂停；“测试版”保持默认或与游戏明确匹配的分支，不凭名称猜测旧版本。
5. 在Steam库中点击Modding Kit的“开始”，在Bannerlord启动器中选择 `Singleplayer`。
6. 进入初始菜单后点击 `Editor`，或按 `Ctrl+E` 打开官方编辑器。

### 4. 安装后应出现的文件

```text
C:\Program Files (x86)\Steam\steamapps\appmanifest_1393600.acf

C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\
├─ bin\
│  └─ Win64_Shipping_wEditor\
├─ XMLEditor\
└─ Tools\
```

首次启动XML Editor时，将游戏目录设置为：

```text
C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord
```

其他XML Editor设置先保持默认；XSLT编辑功能仍属于实验选项，等最小模块骨架通过后再决定是否启用。

### 5. Steam库中搜索不到工具时

按以下顺序排查，不要立即转向第三方下载：

1. 确认库筛选器已经勾选“工具”。
2. 取消“仅显示已安装／可立即游玩”、收藏和动态集合过滤。
3. 清空搜索框后重新搜索完整名称与 `Bannerlord`。
4. 打开Steam顶部“查看 → 隐藏的游戏”，检查是否被误隐藏。
5. 完全退出Steam客户端并重新登录。
6. 确认当前账户实际拥有Bannerlord，而不是只看到另一账户临时共享的游戏。
7. 使用官方Steam应用ID直接打开安装确认窗口：按 `Win+R`，输入：

```text
steam://install/1393600
```

8. 或在浏览器打开：

```text
https://store.steampowered.com/app/1393600/
```

`steam://install/1393600` 只会请求Steam打开对应应用的安装确认，不会绕过账户许可。

### 6. 根据结果继续处理

| 现象 | 下一步 |
|---|---|
| 出现正常安装窗口 | 选择与游戏相同的C盘Steam库并安装 |
| Steam打开但没有反应 | 完全退出Steam后重新登录，再重试应用ID |
| 提示无许可或当前不可用 | 停止操作，保存提示截图，检查账户、发行状态与Steam缓存 |
| 工具被安装到另一个Bannerlord目录 | 不手工复制或覆盖，先记录两个完整路径再诊断 |
| 编辑器启动失败 | 先核对版本、同盘安装、官方文件完整性和第三方DLL模块 |

安装完成后暂不创建模块。先重新执行环境复检，确认版本、目录、War Sails工具内容、XML Editor和官方编辑器都正常，再进入“记录003”的最小模块加载测试。

### 7. 官方资料

- TaleWorlds工具安装与启动FAQ：<https://moddocs.bannerlord.com/asset-management/faq/>
- TaleWorlds XML Editor入门：<https://moddocs.bannerlord.com/bestpractices/xml_editor/>
- Steam安装与文件校验排错：<https://help.steampowered.com/en/faqs/view/21F5-8D5D-0141-7A5E>

---

## 战斗记录 005：Modding Kit 安装复检与目录差异

**记录日期：** 2026-11-01  
**目标：** 确认官方工具已经安装，并修正旧教程对目录结构的过时预期。  
**操作边界：** 只读取 Steam 清单与本地工具目录；后续不由 Codex 操作 Steam、Modding Kit 或游戏界面。

### 1. 安装结果

已确认存在：

```text
C:\Program Files (x86)\Steam\steamapps\appmanifest_1393600.acf
C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_wEditor
C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\XmlEditor\XmlEditor.exe
C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\modding_resources
C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\XmlSchemas
```

Steam Modding Kit 应用信息：

| 项目 | 已确认值 |
|---|---|
| App ID | `1393600` |
| 名称 | `Mount & Blade II: Bannerlord - Modding Kit` |
| Build ID | `24575232` |
| 磁盘占用 | 约 30.9 GB |
| 游戏基线 | Bannerlord `v1.4.8` / Build `24573425` |

### 2. `Tools` 目录差异

当前工具包没有旧教程所列的顶层 `Tools` 文件夹，但编辑器程序、XML Editor、`modding_resources` 与 `XmlSchemas` 均已存在。不能仅以缺少顶层 `Tools` 目录判定安装不完整；本项目以后以当前版本实际文件布局为准。

### 3. 结论

Modding Kit 主体安装完成，能够支持后续 XML、资源和编辑器工作。游戏界面测试仍由用户亲自执行，Codex 只负责模块文件、静态校验、文档和用户主动提供的日志分析。

---

<a id="combat-prototype"></a>

## 战斗记录 006：T2“行省征召兵”单兵种原型

**记录日期：** 2026-11-01  
**版本：** `v0.1.1-t2-prototype`  
**当前状态：** 已通过；2026-11-01 由用户手动完成游戏内验证，未报告问题。

### 1. 本轮范围

只新增一名 T2 兵种，不提前加入 T3、T4、T5，不加入 DLL、Harmony、自定义资产或 War Sails 专属装备。

```text
T1 帝国新兵（imperial_recruit）
  ├─ T2 帝国步兵（原版）
  ├─ T2 帝国弓箭手（原版）
  └─ T2 行省征召兵（wx_provincial_levy，本 MOD 新增）
```

### 2. 对象定义

| 字段 | 当前值 |
|---|---|
| 对象 ID | `wx_provincial_levy` |
| 英文显示名 | `Provincial Levy` |
| 中文工作名 | 行省征召兵 |
| 等级 | 11（T2） |
| 文化 | `Culture.empire` |
| 编组 | `Infantry` |
| 职业 | `Soldier` |
| 后续升级 | `wx_provincial_auxiliary`（自 v0.2.0 起） |

### 3. 技能定位

| 技能 | 数值 |
|---|---:|
| Athletics | 40 |
| Riding | 5 |
| OneHanded | 35 |
| TwoHanded | 15 |
| Polearm | 40 |
| Bow | 10 |
| Crossbow | 10 |
| Throwing | 20 |

该数值以原版帝国 T2 步兵的同阶范围为基准，略微降低单手专精，保留长杆与未来投掷路线的倾向，不制造同阶数值优势。

### 4. 原版装备方案

两套随机装备均使用 SandBoxCore 原版对象：

- 主武器：`imperial_spear_t2`
- 副武器：`empire_sword_1_t2`
- 盾牌：`battered_kite_shield` 或 `old_kite_shield`
- 护具：帝国轻型软甲、布甲、头巾、靴子及少量护腕／围巾
- 平民装备模板：`empire_troop_civilian_template_t1`

### 5. 合并方式

在本 MOD 的 `spnpccharacters.xml` 中使用与 NavalDLC 相同的局部对象合并思路，为 `imperial_recruit` 追加一个 `upgrade_target`。不复制或覆盖原版完整帝国新兵定义，因此原有两条升级路线应继续保留。

### 6. 静态校验结果

| 检查项 | 结果 |
|---|---|
| `NPCCharacters.xsd` | 0 个错误 |
| 新兵种对象数量 | 1 |
| 帝国新兵合并桩数量 | 1 |
| 装备配置数量 | 2 |
| 引用装备数量 | 11 |
| 缺失装备 ID | 0 |
| 平民装备模板 | 已找到 |
| 安装副本与校验副本哈希 | 完全一致 |

### 7. 手动测试结果

- [x] Launcher 显示模块版本 `v0.1.1`，无依赖警告。
- [x] 能进入主菜单与自定义战斗。
- [x] 帝国阵营步兵列表中能找到 `Provincial Levy`。
- [x] 装备能够正常生成，无裸体、空手或粉色材质。
- [x] 能进入战场，武器与盾牌可以正常使用。
- [x] 帝国新兵保留原版升级目标，并新增 `Provincial Levy` 路线。

阶段结论：T2 对象定义、装备、CustomBattle 枚举与 `imperial_recruit` 合并方式均已得到实机验证，可以扩展完整路线。

---

## 战斗记录 007：简体中文与完整 T2–T5 兵种线

**记录日期：** 2026-11-01  
**版本：** `v0.2.0-full-line-prototype`  
**当前状态：** 文件实现、引用检查、XML Schema 与安装一致性校验已完成；等待用户进行完整路线实机测试。

### 1. 最终升级路线

```text
T1 帝国新兵（原版 imperial_recruit）
  └─ T2 行省征召兵（wx_provincial_levy）
       └─ T3 行省辅兵（wx_provincial_auxiliary）
            └─ T4 自由军团战士（wx_free_company_warrior）
                 └─ T5 自由军团老兵（wx_free_company_veteran）
```

原版帝国步兵与弓兵路线不受替换；本路线仍是帝国新兵的第三个升级目标。

### 2. 等级与技能梯度

| 兵种 | 等级 | 跑动 | 单手 | 长杆 | 投掷 |
|---|---:|---:|---:|---:|---:|
| 行省征召兵 | 11 | 40 | 35 | 40 | 20 |
| 行省辅兵 | 16 | 70 | 60 | 70 | 65 |
| 自由军团战士 | 21 | 105 | 95 | 105 | 100 |
| 自由军团老兵 | 26 | 135 | 125 | 135 | 130 |

高阶数值有意将主要预算投向跑动、长杆和投掷；单手能力略低于同阶帝国正规重步兵。

### 3. 装备梯度

- T3：T3 长矛、普通投枪、帝国单手剑／斧、改良鸢盾、软甲。
- T4：帝国 T4 精钢矛、中级投枪、单手剑／斧、强化鸢盾、轻型锁甲。
- T5：帝国 T4 精钢矛、高级投枪、T4 战斧或 T5 帝国剑、强化鸢盾、中型帝国甲。
- 每一级均有两套随机装备配置。
- T5 使用 `empire_legion_a/b`，其躯干防护低于军团步兵的 `imperial_lamellar`，因此不会成为帝国军团步兵的纯粹上位替代。
- 全部装备来自 SandBoxCore，不引用 War Sails 专属物品。

### 4. 简体中文本地化

新增：

```text
ModuleData\Languages\CNs\language_data.xml
ModuleData\Languages\CNs\wx_provincial_free_company_xml-zho-CN.xml
```

| 文本 ID | 简体中文 | 英文后备名称 |
|---|---|---|
| `wx_provincial_levy_name` | 行省征召兵 | Provincial Levy |
| `wx_provincial_auxiliary_name` | 行省辅兵 | Provincial Auxiliary |
| `wx_free_company_warrior_name` | 自由军团战士 | Free Company Warrior |
| `wx_free_company_veteran_name` | 自由军团老兵 | Free Company Veteran |

### 5. 静态校验结果

| 检查项 | 结果 |
|---|---|
| 模块版本 | `v0.2.0` |
| 新兵种数量 | 4 |
| 等级序列 | 11、16、21、26 |
| 自定义升级目标 | 3 个，全部存在 |
| `NPCCharacters.xsd` | 0 个错误 |
| 唯一装备引用 | 47 个 |
| 缺失装备 ID | 0 |
| 平民装备模板 | T1、T2、T3 均存在 |
| 中文文本键 | 4 个，全部有翻译 |
| 中文语言文件路径 | 有效 |
| NavalDLC 专属引用 | 0 |
| 安装副本与校验副本哈希 | 6 个文件全部一致 |

覆盖安装前已在工作区保留实机通过的 `v0.1.1` 文件备份，便于故障对照；没有修改任何官方模块文件。

### 6. v0.2.0 实机验收项目

- [ ] Launcher 显示 `v0.2.0`，无依赖警告。
- [ ] 简体中文环境正确显示四个中文兵种名。
- [ ] 自定义战斗能选择全部四个兵种。
- [ ] T3、T4、T5 的两套随机装备均无缺失、裸体或材质错误。
- [ ] 四级兵种均能正常进入战场并使用长矛、盾牌、单手武器和投枪。
- [ ] 测试战役升级链完整且顺序正确。
- [ ] 原版帝国步兵、弓兵与 War Sails 水手路线没有丢失。
- [ ] 升级、保存、退出并重新加载测试存档后，自定义兵种仍然存在。

---

## 战斗记录 008：GitHub 公开仓库与版本历史建立

**记录日期：** 2026-11-01
**仓库名称：** `bannerLoard-mod`
**仓库状态：** 公开仓库已创建，`main` 分支、两个版本标签与本地历史均已完成校验和对齐。

### 1. 仓库位置

- GitHub：<https://github.com/SuperShuangPowder/bannerLoard-mod>
- 本地仓库：`D:\CodexProject\独立游戏开发之梦与计划\一阶段-MOD制作\bannerLoard-mod`
- 项目目录：`bannerlord-wx-provincial-free-company`
- 正式模块源码：`bannerlord-wx-provincial-free-company\src\WXProvincialFreeCompany`

仓库名称沿用创建时确认的拼写 `bannerLoard-mod`。游戏安装目录继续只承担部署与实机测试，不作为正式源码和开发历史的唯一存放位置。

### 2. 仓库结构

```text
bannerLoard-mod/
├─ .gitignore
├─ README.md
└─ bannerlord-wx-provincial-free-company/
   ├─ README.md
   ├─ docs/
   │  ├─ 额外知识获取-git创建.md
   │  └─ 骑砍2兵种拓展MOD开发日志.md
   └─ src/
      └─ WXProvincialFreeCompany/
```

其中 `src\WXProvincialFreeCompany` 是可部署到 Bannerlord `Modules` 目录的完整模块；`docs` 保存随仓库发布的开发记录与 Git 操作教程。

### 3. Git 历史与版本标签

| 版本 | 提交 | 内容 | 状态 |
|---|---|---|---|
| `v0.1.1` | `0188e08` | 已通过实机测试的 T2“行省征召兵”原型 | 已建立标签 |
| `v0.2.0` | `8031ea8` | 完整 T2–T5 兵种线与简体中文本地化 | 已建立标签，等待完整实机验收 |

历史从已经实测的 `v0.1.1` 起步，没有把游戏安装目录、临时验证文件或无意义的中间副本混入首个版本。

### 4. 发布前检查

| 检查项 | 结果 |
|---|---|
| 仓库可见性 | `PUBLIC` |
| 默认分支 | `main` |
| 远端 `main` 与本地 `HEAD` | 均为 `8031ea871350878ec1b74082680979d8ea81e3e8` |
| 版本标签 | `v0.1.1`、`v0.2.0` 均存在 |
| 本地跟踪关系 | `main...origin/main`，工作区干净 |
| 存档、日志、转储、压缩包、DLL、PDB | 未收录 |
| 账号令牌、私钥、密码等敏感内容 | 未发现 |
| 官方游戏资产 | 未收录 |

仓库当前没有添加开源许可证。公开可见只表示任何人都可以浏览代码，不代表自动授予复制、修改或再发布权利；是否选择许可证留待准备正式发布时决定。

### 5. 后续开发约定

1. 以后先在本地仓库的 `src\WXProvincialFreeCompany` 中维护正式源码。
2. 需要实机测试时，再将模块部署到游戏的 `Modules\WXProvincialFreeCompany`。
3. 每个可复现的里程碑先完成静态检查和手动测试，再创建 Git 提交与版本标签。
4. 本文件继续作为完整、持续更新的开发日志；仓库 `docs` 中的同名文件是随版本提交的公开快照，后续发布时一并同步。
5. 不提交存档、崩溃日志、Modding Kit 临时文件、官方资产或任何账号凭据。

---

<a id="combat-regional-structure"></a>

## 战斗记录 009：行省自由军的四地域分支与长期设计构想

**记录日期：** 2026-11-01
**记录性质：** 设计储备，尚未进入代码实现。
**阶段目标：** 在当前单线兵种原型的基础上，同时探索纵向深化与横向扩展，使 MOD 从“新增一条兵种线”逐渐形成具有独立主题的地方军事体系。

### 1. 核心设计愿景

> 让非帝国玩家进入帝国领土后，能够招募一支保留帝国组织能力、吸收边境民族战法、但不效忠帝国皇权的地方武装。

后续设计应围绕三个核心特征展开：

1. **帝国底色：** 保留帝国式组织、训练和部分装备风格。
2. **边境适应：** 根据不同方向的邻国、地貌与长期战争经验发展地域战法。
3. **自由军实用主义：** 不追求最昂贵、最正统的装备，而是强调容易组织、混编互补与服务不同阵营的合理性。

地域兵不应成为邻国正规军的直接复制品。设计公式暂定为：

```text
帝国的组织方式 + 边境地区的装备习惯 + 自由军的实用主义
```

### 2. 两条并行发展方向

#### 方向 A：纵向深化

- 继续完善单个兵种的技能、装备、升级成本和战场定位。
- 让每一级在视觉和作战能力上都能体现成长，而不是单纯增加数值。
- 为每个分支设置明确优势与明确弱点，避免成为帝国正规军的全面上位替代。
- 通过实机测试调整 AI 使用武器、阵型表现、随机装备与战损交换效率。

#### 方向 B：横向扩展

- 保留 T2“行省征召兵”作为共同起点。
- 从 T3 开始分为北、南、东、西四种地域训练方向。
- 每条分支独立成长至 T5，并承担混合军队中的一种战术职责。
- 暂不设置 T6，强调自由军的战术互补，而不是依靠单个顶级兵种碾压正规军。

### 3. 四地域兵种树草案

```text
T1 帝国新兵（原版）
└─ T2 行省征召兵
   ├─ T3 北境行省辅兵
   │  └─ T4 北境盾斧兵
   │     └─ T5 北境自由军盾卫
   │
   ├─ T3 南境行省辅兵
   │  └─ T4 南境标枪兵
   │     └─ T5 南境自由军游击者
   │
   ├─ T3 东境行省辅兵
   │  └─ T4 东境骑乘斥候
   │     └─ T5 东境自由骑兵
   │
   └─ T3 西境行省辅兵
      └─ T4 西境弩盾兵
         └─ T5 西境自由军弩卫
```

以上名称只用于表达方向，正式名称将在兵种设计表阶段继续调整。

### 4. 四分支的战术定位

| 分支 | 地域灵感 | 战场职责 | 主要装备 | 主要优势 | 明确弱点 |
|---|---|---|---|---|---|
| 北境 | 斯特吉亚及北部边境 | 持盾扛线、近战混战 | 圆盾、斧、短矛、较厚护具 | 盾战与近距离持续作战 | 远程能力和追击能力有限 |
| 南境 | 阿塞莱及南部边境 | 投掷骚扰、掩护与反骑 | 轻盾、长矛、多组标枪、轻甲 | 跑动、投掷和战场机动 | 护甲较轻，不适合长时间正面消耗 |
| 东境 | 库赛特及东部草原 | 侦察、侧翼、追击 | 马匹、长矛、标枪、轻型鳞甲 | 战略与战场机动性 | 升级成本高，正面肉搏能力有限 |
| 西境 | 瓦兰迪亚及西部边境 | 远程穿甲、阵地支援 | 弩、盾、短兵器、中型护具 | 对重甲目标和静态防线效果好 | 装填慢，被骑兵贴近后较脆弱 |

四条线组合后的预期协同关系：

- 北境兵维持正面战线。
- 南境兵在阵线前方进行标枪骚扰，并保护侧翼、阻拦骑兵。
- 东境骑兵执行侦察、包抄、驱赶远程兵和战后追击。
- 西境弩兵从阵线后方提供稳定的远程穿甲火力。
- 玩家与同伴负责统帅、重骑兵或精锐突击等缺失位置。

单个分支不要求独立解决所有问题；四种地方兵混编后才形成完整的行省自由军。

### 5. 视觉识别原则

在暂不制作新模型和贴图的阶段，优先使用原版资产建立清晰轮廓：

| 分支 | 远距离视觉特征 |
|---|---|
| 北境 | 圆盾、斧、毛皮和较宽重的步兵轮廓 |
| 南境 | 轻甲、小盾、标枪和更轻快的轮廓 |
| 东境 | 马匹、长矛、轻型鳞甲和骑乘装备 |
| 西境 | 弩、鸢盾或大盾、较统一的头盔 |
| 共通底色 | 保留部分帝国头盔、软甲、盾纹与装备搭配方式 |

随机装备暂定遵循“约七成固定轮廓、约三成装备变化”的原则：既表现自由军装备来源复杂，又保证玩家能够迅速辨认不同分支。

### 6. 当前兵种线的候选归属

当前 `v0.2.0` 的长矛、鸢盾、标枪和较轻护甲组合，与南境机动投矛路线最接近，因此可以作为未来南境线的设计基础。

正式改造前需要决定：

- **优先结构整洁：** 在尚未正式发布、没有重要长期存档时，重新建立明确的地域兵种 ID。
- **优先存档兼容：** 保留现有 `wx_provincial_auxiliary`、`wx_free_company_warrior` 和 `wx_free_company_veteran` ID，仅修改名称、装备与定位。

`v0.2.0` 已通过 Git 标签保留，因此即使未来重构，也能随时查看和恢复当前单线版本。

### 7. 纯 XML 方案的边界

如果仅为 T2 添加四个 `upgrade_target`：

- 玩家可以在任何地点选择任意地域分支。
- NPC 领主也可能在四个分支之间自行升级。
- 游戏不会因为部队身处北方，就自动只允许北境升级路线。
- 四种“地域”在此阶段代表训练流派和文化风格，而不是真正受地图位置限制的招募来源。

这一阶段可以将其解释为：玩家或领主为行省征召兵选择不同边境军团的训练方式。

若要实现“北方村庄只提供北境兵、南方村庄只提供南境兵”，则需要进一步研究：

- 定居点与村庄的志愿兵模板。
- 不同地区的独立招募根节点。
- 基于城镇、村庄或文化的地域判断。
- NPC 领主是否应遵循地区化升级逻辑。
- 是否需要 C# 行为控制，而不再局限于 XML 数据修改。

### 8. 中长期玩法构想

#### 8.1 地域招募

- 北方帝国村庄或城镇附近主要出现北境自由军。
- 南部边境主要提供南境标枪兵。
- 东部草原边境提供骑乘辅助兵。
- 西部边境提供弩手与盾兵。
- 酒馆或佣兵渠道可以偶尔提供已经成型的自由军小队。

#### 8.2 战俘转化

允许非帝国玩家将俘虏的帝国士兵、逃兵或投降者转化为自由军，例如：

```text
帝国俘虏／脱籍士兵
└─ 脱籍兵
   └─ 行省征召兵或指定地域分支
```

该机制非常符合最初“使用帝国当地人组织非帝国武装”的游玩需求，但预计需要 C# 逻辑，暂列为中长期目标。

#### 8.3 可选 War Sails 扩展

核心 MOD 继续不依赖 NavalDLC；未来可以单独建立可选子模块：

```text
WXProvincialFreeCompany          核心行省自由军，不依赖 DLC
WXProvincialFreeCompanyNaval     沿海与海战扩展，需要 War Sails
```

海战扩展候选内容包括沿海行省水手、登船矛手、舰上弩手和自由军陆战老兵。这样既不限制未购买 DLC 的玩家，又能利用当前 War Sails 环境继续拓展。

#### 8.4 部队背景与非正式称号

可以为不同分支设计短背景、佣兵团称号或战场俗称，例如：

- 北境“鸦盾团”
- 南境“赤沙投矛队”
- 东境“苍鹰骑手”
- 西境“灰墙弩手”

这些称号暂不一定直接作为正式兵种名，可以用于文档、百科说明、酒馆招募文本或未来事件系统，为兵种提供人文记忆点。

### 9. 工作量与范围控制

完整四分支意味着：

- 1 个共同 T2 加上 12 个地域 T3–T5，共计 13 个自定义兵种。
- 如果每级保留两套随机装备，至少需要维护 24 套地域装备配置。
- 需要同步处理 12 个地域兵种的中文文本、属性、升级成本和 CustomBattle 表现。
- 东境骑兵还要验证马匹需求、骑乘技能、武器 AI 与升级条件。
- NPC 自动升级后可能形成难以预测的混编比例，需要单独观察。

因此不采用“一次性做完十二个兵种”的方式，而是先建立一条高质量参考线，再逐条扩展。

### 10. 建议开发顺序

1. 完成当前 `v0.2.0` 中文、自定义战斗、升级链和存档实机验收。
2. 编写四条路线的详细兵种设计表，确定名称、技能、装备、成本、优势和弱点。
3. 决定地域兵种 ID 是否重构，以及对旧测试存档的兼容策略。
4. 将现有路线深化为南境参考线，建立统一设计标准。
5. 制作北境圆盾步兵线，首先验证 T2 多分支升级是否正常。
6. 制作东境骑乘辅助兵线，单独验证马匹与骑兵 AI。
7. 制作西境弩兵线，完成四兵种协同体系。
8. 四条路线稳定后，再研究真正的地域招募、NPC 地域升级与战俘转化。
9. 最后评估是否制作独立的 War Sails 可选扩展。

阶段决策：本轮只保存设计灵感，不立即修改 `v0.2.0` 源码。下一项实际工作仍然是完成当前版本的手动验收，避免尚未验证的基线与大规模扩展同时变化。

---

## 战斗记录 010：v0.3.0 四地域分支结构原型

**记录日期：** 2026-11-02
**版本：** `v0.3.0-regional-skeleton`
**当前状态：** 十二个地域占位兵种已经实现，静态校验与部署一致性检查通过，等待用户进行手动结构验收。

### 1. v0.2.0 实机反馈

用户已完成 `v0.2.0` 的阶段性实机验证：

- 新增兵种线能够在战役中出现并正常升级。
- 四个新增兵种能够在自定义战斗界面中找到。
- 中文名称、模块加载与基础战斗均可正常工作。
- 功能层面的 T2–T5 单线原型验收通过。

同时发现两个明确的设计问题：

1. 新兵种大量复用帝国正规军装备，远距离轮廓与帝国军几乎相同，缺少独立辨识度。
2. 新兵种与同等级帝国近战兵正面对抗时明显偏弱，当前配置更像正规军的弱化版本，而不是具有明确战术补偿的侧向选择。

数据复核显示，高阶自由军护甲显著低于帝国老兵与军团步兵，同时单手能力略低；增加的长杆和投掷能力尚未形成足以补偿正面战损的优势。因此 `v0.2.0` 的结论是：**功能通过，视觉与平衡不通过。**

### 2. 本轮开发决策

采用“先横向铺开结构，再纵向深化内容”的开发顺序：

- 保留 T2 `wx_provincial_levy` 作为共同起点。
- 从 T3 开始分为北境、南境、东境、西境四条路线。
- 每条路线包含 T3、T4、T5，共新增十二个地域兵种对象。
- 同等级地域兵暂时完全复制 `v0.2.0` 对应兵种的技能、装备、随机装备配置、战斗分组与平民模板。
- 本版本只验证数据对象、升级关系、文本、CustomBattle 枚举与存档结构，不处理视觉和平衡差异。

### 3. v0.3.0 兵种树

```text
T1 帝国新兵（原版 imperial_recruit）
└─ T2 行省征召兵（wx_provincial_levy）
   ├─ T3 北境行省辅兵（wx_northern_provincial_auxiliary）
   │  └─ T4 北境自由军战士（wx_northern_free_company_warrior）
   │     └─ T5 北境自由军老兵（wx_northern_free_company_veteran）
   ├─ T3 南境行省辅兵（wx_southern_provincial_auxiliary）
   │  └─ T4 南境自由军战士（wx_southern_free_company_warrior）
   │     └─ T5 南境自由军老兵（wx_southern_free_company_veteran）
   ├─ T3 东境行省辅兵（wx_eastern_provincial_auxiliary）
   │  └─ T4 东境自由军战士（wx_eastern_free_company_warrior）
   │     └─ T5 东境自由军老兵（wx_eastern_free_company_veteran）
   └─ T3 西境行省辅兵（wx_western_provincial_auxiliary）
      └─ T4 西境自由军战士（wx_western_free_company_warrior）
         └─ T5 西境自由军老兵（wx_western_free_company_veteran）
```

### 4. 版本与存档策略

- 开发分支：`feature/v0.3.0-regional-skeleton`
- 模块版本：`v0.3.0`
- `v0.2.0` 实机通过副本备份：`C:\Users\WANGXINYU858\Documents\Codex\2026-10-11\new-chat\backups\WXProvincialFreeCompany-v0.2.0-functional-tested`
- `v0.2.0` Git 标签继续保留单线原型历史。
- 三个旧通用 ID `wx_provincial_auxiliary`、`wx_free_company_warrior`、`wx_free_company_veteran` 已从新版本中移除。
- `v0.3.0` 不以兼容包含旧 T3–T5 兵种的测试存档为目标，手动验收建议使用新测试存档。

### 5. 静态校验结果

| 检查项 | 结果 |
|---|---|
| 模块版本 | `v0.3.0` |
| 自定义兵种总数 | 13：共同 T2 1 个，地域 T3/T4/T5 各 4 个 |
| T2 地域升级目标 | 4 个 |
| 自定义升级连接 | 12 条 |
| 缺失升级目标 | 0 |
| 重复兵种 ID | 0 |
| 旧通用 T3–T5 ID 残留 | 0 |
| 简体中文文本键 | 13 个，全部存在 |
| 唯一装备引用 | 47 个 |
| 缺失装备 ID | 0 |
| T3 同阶配置数量 | 1，四地域完全相同 |
| T4 同阶配置数量 | 1，四地域完全相同 |
| T5 同阶配置数量 | 1，四地域完全相同 |
| `NPCCharacters.xsd` | 0 个错误 |
| NavalDLC 专属引用 | 0 |
| 仓库源码与安装副本 | 6 个文件哈希全部一致 |

### 6. v0.3.0 手动验收项目

- [ ] Launcher 显示模块版本 `v0.3.0`，无依赖警告。
- [ ] 简体中文环境正确显示共同 T2 和十二个地域兵种名称。
- [ ] T2“行省征召兵”同时显示北、南、东、西四个 T3 升级目标。
- [ ] 四条路线均能从 T3 独立升级到对应的 T4 和 T5，不发生串线。
- [ ] 自定义战斗中能够找到十二个地域兵种。
- [ ] 十二个地域兵种均可正常生成装备并进入战场。
- [ ] 新测试存档中能够混编四条路线，保存、退出并重新加载后兵种仍然存在。
- [ ] 原版帝国步兵、弓兵和 War Sails 水手升级路线没有丢失。
- [ ] 观察 NPC 领主在面对四个 T3 目标时的自动升级分配，但本轮不要求形成地域化选择。

### 7. 已知占位问题

- 四条地域路线目前只有 ID、名称和升级关系不同。
- 十二个兵种继续沿用 `v0.2.0` 的视觉雷同与同阶近战偏弱问题。
- 地域名称代表训练流派，并未与地图招募位置绑定。
- 本轮不以兵种强度、外观差异或最终名称作为验收标准。

---

## 战斗记录 011：v0.3.0 功能分支推送与 Pull Request 合并

**记录日期：** 2026-11-02
**Pull Request：** <https://github.com/SuperShuangPowder/bannerLoard-mod/pull/1>
**当前状态：** `v0.3.0-regional-skeleton` 已通过 Pull Request 合并到远端与本地 `main`；功能分支保留，尚未创建 `v0.3.0` 标签。

### 1. 详细功能提交

在推送前，将最初的本地提交改写为带完整正文的正式提交：

- 功能提交：`43b2b94523db9e5d6ef9a15905f7eaf563de4509`
- 提交标题：`feat: add v0.3.0 regional troop tree skeleton`
- GitHub：<https://github.com/SuperShuangPowder/bannerLoard-mod/commit/43b2b94523db9e5d6ef9a15905f7eaf563de4509>

提交正文完整记录了：

- 共同 T2 与北、南、东、西四条 T3–T5 地域路线。
- 十二个地域占位兵种及同阶配置完全相同的设计目的。
- 模块版本、本地化、README、CHANGELOG 与开发日志更新。
- 13 个兵种、12 条升级连接、47 个装备引用和 Schema 检查结果。
- `v0.2.0` 的视觉、平衡遗留问题。
- 旧通用 T3–T5 ID 退出后对测试存档的兼容性影响。

### 2. 功能分支推送

- 本地分支：`feature/v0.3.0-regional-skeleton`
- 远端分支：`origin/feature/v0.3.0-regional-skeleton`
- 推送完成后，本地与远端功能分支均指向 `43b2b94523db9e5d6ef9a15905f7eaf563de4509`。
- 功能分支没有在合并后删除，便于继续审阅与追溯。

### 3. Pull Request 内容与检查

创建 PR #1：

- 标题：`feat: add v0.3.0 regional troop tree skeleton`
- 基础分支：`main`
- 来源分支：`feature/v0.3.0-regional-skeleton`
- 提交数量：1
- 冲突状态：`MERGEABLE`
- 合并状态：`CLEAN`
- 自动检查：仓库当前没有配置额外的 GitHub Actions 检查。

PR 描述包含变更摘要、静态校验结果、已知占位问题与存档兼容性说明，避免仅凭简短标题直接合并。

### 4. 合并结果

按照用户确认，PR #1 使用普通 merge commit 合入 `main`：

- 合并提交：`46eb3bc7cd6e23ceceb0acdb30cbaedd08a4e62a`
- GitHub：<https://github.com/SuperShuangPowder/bannerLoard-mod/commit/46eb3bc7cd6e23ceceb0acdb30cbaedd08a4e62a>
- PR 状态：`MERGED`
- 本地 `main`：`46eb3bc7cd6e23ceceb0acdb30cbaedd08a4e62a`
- 远端 `origin/main`：`46eb3bc7cd6e23ceceb0acdb30cbaedd08a4e62a`
- 本地工作区：干净，`main...origin/main`

当前历史关系：

```text
46eb3bc  main / origin/main
└─ Merge v0.3.0 regional troop tree skeleton
   ├─ 43b2b94  feature/v0.3.0-regional-skeleton
   └─ 8031ea8  v0.2.0
```

### 5. 当前版本边界

- `main` 已包含 `v0.3.0-regional-skeleton` 的全部源码与文档。
- 游戏模块目录已经部署 `v0.3.0`，可以继续进行手动测试。
- `v0.1.1`、`v0.2.0` 标签均未改动。
- 尚未创建 `v0.3.0` 标签；该标签是否建立，留待手动结构验收或下一次明确决定。
- 合并到 `main` 不代表视觉和平衡问题已经解决；四地域兵种仍是结构占位配置。
- 如果手动测试发现问题，将通过新的修复提交处理，不改写已经合并的 Git 历史。

---

<a id="combat-branch-development"></a>

## 战斗记录 012：v0.3.1 北境自由军盾斧兵线

**记录日期：** 2026-11-02
**版本：** `v0.3.1-northern-line`
**当前状态：** 北境 T3–T5 已完成第一轮装备、技能和名称深化，静态校验与部署一致性检查通过，等待用户进行视觉与战斗实机验收。

### 1. 本轮设计目标

北境线作为四地域路线中的第一条正式参考线，采用以下视觉公式：

```text
帝国军事骨架 + 自由军共同组织感 + 北境战术装备
```

具体边界：

- 胸甲、肩甲、护臂、靴子和大部分头盔维持帝国体系。
- 圆盾与单手斧承担最醒目的北境特征。
- 投矛保留帝国步兵的训练痕迹。
- 北境头盔只出现在 T5 三套随机装备中的一套，避免整支部队被误认为斯特吉亚正规军。
- 不使用完整的斯特吉亚胸甲套装，保证帝国身份仍是视觉主体。

### 2. 正式兵种名称

| 等级 | 内部 ID | 中文名称 | 英文后备名称 |
|---|---|---|---|
| T3 | `wx_northern_provincial_auxiliary` | 北境行省辅兵 | Northern Provincial Auxiliary |
| T4 | `wx_northern_free_company_warrior` | 北境自由军盾斧兵 | Northern Free Company Shield-Axeman |
| T5 | `wx_northern_free_company_veteran` | 北境自由军盾卫 | Northern Free Company Shieldguard |

### 3. 技能定位

| 兵种 | 跑动 | 单手 | 双手 | 长杆 | 投掷 | 定位 |
|---|---:|---:|---:|---:|---:|---|
| 北境行省辅兵 | 70 | 70 | 40 | 50 | 50 | 轻型圆盾斧兵，具备少量接敌前投掷能力 |
| 北境自由军盾斧兵 | 105 | 105 | 60 | 75 | 70 | 中型盾斧步兵，负责正面持线和破盾 |
| 北境自由军盾卫 | 135 | 135 | 80 | 90 | 90 | 重型盾斧老兵，使用双组帝国投矛削弱接敌目标 |

相比 `v0.3.0` 占位配置，本线提高单手能力并降低未充分发挥的长杆与投掷预算。设计目标不是全面超过军团步兵，而是凭借斧头破盾、圆盾阵线和较高跑动形成近战侧向选择。

### 4. 装备设计

#### T3 北境行省辅兵

- 帝国软甲、帝国肩甲、帝国靴和帝国手套作为基础。
- 使用 `sturgia_axe_3_t3`。
- 两套装备分别使用 `northern_round_shield` 与 `viking_round_shield`。
- 携带一组轻型投枪。
- 一套使用普通软帽，一套使用帝国鼻护盔。

#### T4 北境自由军盾斧兵

- 两套装备分别使用帝国 `empire_legion_a` 与 `empire_legion_b` 军团甲。
- 使用帝国重型鼻护盔、帝国肩甲、帝国靴和帝国手套。
- 使用 `sturgia_axe_4_t4`。
- 两套装备分别使用 `heavy_round_shield` 与 `leather_round_shield`。
- 携带一组帝国投矛，不再携带可能影响近距离武器选择的长矛。

#### T5 北境自由军盾卫

- 三套装备全部使用帝国 `imperial_lamellar`、帝国肩甲、板条护臂与板条靴。
- 使用 `sturgia_axe_5_t5`。
- 使用重型圆盾、大型圆盾或诺斯圆盾。
- 携带两组帝国投矛，与帝国正规军训练痕迹保持联系。
- 三套装备中两套使用帝国金属条头盔，一套使用 `sturgian_helmet_closed`。

### 5. 护甲梯度

数值为一套装备中各护甲部件相加后的头／身／臂／腿防护：

| 兵种 | 装备套装 | 头 | 身 | 臂 | 腿 |
|---|---:|---:|---:|---:|---:|
| 北境行省辅兵 | 1 | 14 | 24 | 17 | 13 |
| 北境行省辅兵 | 2 | 36 | 22 | 17 | 13 |
| 北境自由军盾斧兵 | 1 | 46 | 36 | 37 | 23 |
| 北境自由军盾斧兵 | 2 | 48 | 42 | 37 | 23 |
| 北境自由军盾卫 | 1 | 47 | 54 | 47 | 46 |
| 北境自由军盾卫 | 2 | 47 | 54 | 47 | 46 |
| 北境自由军盾卫 | 3 | 53 | 54 | 47 | 46 |

T5 身、臂、腿防护与帝国军团步兵处于同一档次；其差异主要来自斧头、圆盾、较高跑动以及盾牌覆盖方式，而不是通过削弱护甲制造“特色”。

### 6. 版本与备份

- 开发分支：`feature/v0.3.1-northern-line`
- 模块版本：`v0.3.1`
- `v0.3.0` 部署副本备份：`C:\Users\WANGXINYU858\Documents\Codex\2026-10-11\new-chat\backups\WXProvincialFreeCompany-v0.3.0-regional-skeleton`
- 南境、东境、西境三个分支与 `v0.3.0` 完全一致，没有在本轮被修改。

### 7. 静态校验结果

| 检查项 | 结果 |
|---|---|
| 模块版本 | `v0.3.1` |
| 自定义兵种 | 13 个 |
| 自定义升级连接 | 12 条，缺失目标 0 |
| 本地化缺失 | 0 |
| 唯一装备引用 | 64 个 |
| 缺失装备 ID | 0 |
| 北境地域兵种 | 3 个 |
| 北境 T5 随机装备 | 3 套 |
| 北境 T5 北境头盔配置 | 1 套 |
| 南、东、西分支与 v0.3.0 对比 | 完全一致 |
| `NPCCharacters.xsd` | 0 个错误 |
| 仓库源码与安装副本 | 6 个文件哈希全部一致 |

### 8. v0.3.1 手动验收项目

- [ ] Launcher 显示模块版本 `v0.3.1`，无依赖警告。
- [ ] 北境 T4、T5 正确显示“北境自由军盾斧兵”和“北境自由军盾卫”。
- [ ] 北境三种兵种能够正常出现在战役与自定义战斗中。
- [ ] 圆盾、斧头、帝国胸甲与头盔能够正确生成，无裸体或缺失材质。
- [ ] 北境 T5 三套随机装备中，帝国头盔仍是主体，北境头盔只作为少数变体。
- [ ] AI 能在接敌前使用投枪，并在近战时可靠切换到单手斧与圆盾。
- [ ] 北境 T4、T5 与同阶帝国近战兵交战时，不再呈现单方面溃败。
- [ ] 北境路线从 T3 到 T5 升级正常，南、东、西路线不受影响。
- [ ] 新测试存档保存并重新加载后，北境兵种仍然存在。

阶段说明：本轮只完成静态与部署验证，没有启动 Steam、Modding Kit、Launcher 或游戏；所有实机项目仍由用户手动完成。

---

## 战斗记录 013：v0.3.2 行省征召兵与战役费用机制

**记录日期：** 2026-11-02
**版本：** `v0.3.2-provincial-levy`
**当前状态：** T2 行省征召兵已改造为无盾、轻甲、双手武器主导的边境役卒；静态校验与部署一致性检查通过，等待用户进行实机验收。

### 1. 叙事定位

> 行省征召兵来自帝国边境的农民、猎户、矿工与樵夫。帝国军需体系不愿为他们提供制式盾牌和短剑，只发放沉重的双手武器、征用农具与少量投掷物，命令他们在正规军接战前消耗敌人。

该定位同时解释：

- 他们仍然穿着帝国软甲、布衣、头巾与靴子，属于帝国行省居民。
- 他们没有帝国正规步兵的鸢盾、制式剑和稳定持线能力。
- 他们并非单纯的弱化兵，而是伤害较高、生存率较低的低阶冲击兵。
- 从 T2 升级到四种 T3 地域辅兵，代表这些被当作炮灰的役卒终于获得正式装备与战术身份。

### 2. 技能调整

| 技能 | v0.3.1 | v0.3.2 | 说明 |
|---|---:|---:|---|
| 跑动 | 40 | 50 | 提高无盾冲击与接敌能力 |
| 骑术 | 5 | 5 | 保持徒步兵定位 |
| 单手 | 35 | 10 | 不再接受制式剑盾训练 |
| 双手 | 15 | 50 | 双手斧与双手锤的主要技能 |
| 长杆 | 40 | 45 | 兼容双手钩镰枪 |
| 弓 | 10 | 5 | 不承担弓兵职责 |
| 弩 | 10 | 5 | 不承担弩兵职责 |
| 投掷 | 20 | 35 | 允许部分役卒进行接敌前投掷 |

### 3. 三套随机装备

#### 套装 1：征用长斧兵

- `simple_sparth_axe_t2`：双手长斧。
- `western_javelin_1_t2`：一组轻型标枪。
- 帝国步兵软甲、皮头巾、帝国靴与简易护腕。
- 护甲合计：头 9／身 15／臂 8／腿 12。

#### 套装 2：民用重锤役卒

- `peasant_maul_t1_2`：民用双手锤。
- `throwing_stone`：石块。
- 帝国布帽、带肩垫束腰衣、围巾与帝国靴。
- 护甲合计：头 2／身 8／臂 2／腿 7。

#### 套装 3：边境钩镰兵

- `billhook_polearm_t2`：双手钩镰枪。
- 不携带投掷武器。
- 帝国布帽、帝国步兵软甲、帝国靴与简易护腕。
- 护甲合计：头 6／身 16／臂 8／腿 13。

三套装备均没有盾牌、单手剑或其他单手副武器。投掷装备只出现在其中两套，表现军需分配不完整的临时征召队。

### 4. 招募费与维护费机制调查

本地 `v1.4.8` 的 `NPCCharacters.xsd` 中不存在 `wage`、`recruitment_cost`、`salary` 等可直接写入兵种 XML 的字段。

原版费用由 `TaleWorlds.CampaignSystem.GameComponents.DefaultPartyWageModel` 统一计算。

#### 4.1 维护费

`GetCharacterWage` 按兵阶返回基础日薪：

| 兵阶 | T0 | T1 | T2 | T3 | T4 | T5 | T6 | 更高／默认 |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| 基础日薪 | 1 | 2 | 3 | 5 | 8 | 12 | 17 | 23 |

兵阶由默认角色数值模型根据等级计算；普通非英雄兵种的核心关系为：

```text
Tier = ceil((Level - 5) / 5)
```

行省征召兵等级为 11，因此属于 T2，基础日薪为每天 3 第纳尔。`occupation="Mercenary"` 会使工资乘以 1.5；本兵种保持 `occupation="Soldier"`，不触发该倍率。

#### 4.2 招募费

`GetTroopRecruitmentCost` 首先按照等级区间给出基础价格：

| 等级上限 | 1 | 6 | 11 | 16 | 21 | 26 | 31 | 36 | 更高 |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 基础招募费 | 10 | 20 | 50 | 100 | 200 | 400 | 600 | 1000 | 1500 |

行省征召兵等级为 11，因此基础招募费为 50 第纳尔。

其他调整包括：

- 骑兵会根据等级额外计入 150 或 500 第纳尔的马匹成本。
- `Mercenary`、`Gangster`、`CaravanGuard` 等 occupation 会额外增加两倍基础价，最终形成三倍基础招募费。
- 买方英雄的技能、文化特性和相关 Perk 还可能进一步调整最终价格。

### 5. 本轮费用决策

- `v0.3.2` 维持等级 11 与 `occupation="Soldier"`。
- 继续使用原版 T2 的基础招募费 50 和基础日薪 3。
- 不通过修改等级或 occupation 间接操纵价格，因为这会同时影响兵阶、经验、战斗与其他系统。
- 当前 XML-only 模块无法为单一兵种直接设定特殊费用。
- 若未来需要独立费用，需加入 DLL，并对 `PartyWageModel` 或相关调用进行兵种 ID 特判；这会增加版本兼容和其他 MOD 冲突风险，本轮不引入。

### 6. 版本与备份

- 开发分支：`feature/v0.3.2-provincial-levy`
- 模块版本：`v0.3.2`
- `v0.3.1` 部署副本备份：`C:\Users\WANGXINYU858\Documents\Codex\2026-10-11\new-chat\backups\WXProvincialFreeCompany-v0.3.1-northern-line`
- 除共同 T2、模块版本与文档外，其他十二个兵种与 `v0.3.1` 完全一致。

### 7. 静态校验结果

| 检查项 | 结果 |
|---|---|
| 模块版本 | `v0.3.2` |
| T2 随机装备 | 3 套 |
| T2 盾牌 | 0 |
| T2 单手副武器 | 0 |
| 三套主武器类型 | 双手斧、双手锤、双手长杆 |
| 自定义兵种总数 | 13 个 |
| 自定义升级连接 | 12 条 |
| 唯一装备引用 | 66 个 |
| 缺失装备 ID | 0 |
| 其他十二个兵种与 v0.3.1 对比 | 完全一致 |
| `NPCCharacters.xsd` | 0 个错误 |
| 仓库源码与安装副本 | 6 个文件哈希全部一致 |

### 8. v0.3.2 手动验收项目

- [ ] Launcher 显示模块版本 `v0.3.2`，无依赖警告。
- [ ] 战役与自定义战斗中能够找到 T2 行省征召兵。
- [ ] 三套随机装备均能正确生成，无盾牌、单手副武器、裸体或缺失材质。
- [ ] AI 能正确使用双手斧、双手锤和钩镰枪。
- [ ] 携带投掷物的役卒能够在接敌前使用标枪或石块，并正常切换回双手武器。
- [ ] 对抗同阶帝国步兵时具有一定杀伤交换能力，但生存率和持线能力明显更低。
- [ ] 面对弓弩时更脆弱，但不至于完全无法培养到 T3。
- [ ] T2 仍能正常升级到北、南、东、西四个 T3 目标。
- [ ] 战役招募界面中的基础费用符合 T2 原版模型，队伍维护费没有异常。
- [ ] 新测试存档保存并重新加载后，兵种与升级关系仍然存在。

阶段说明：本轮只完成本地程序集分析、静态校验与部署，没有启动 Steam、Modding Kit、Launcher 或游戏；所有实机项目仍由用户手动完成。

---

## 战斗记录 014：v0.3.3 北境毛皮肩甲与帝国远程精锐军容

**日期：** 2026-11-03
**目标：** 在不破坏帝国军事骨架的前提下强化北境线的寒地边疆辨识度，并让原版帝国 T5 远程兵使用与其精锐身份相符的禁卫射手盔。
**环境：** Bannerlord `v1.4.8`、War Sails / NavalDLC `v1.2.8`、模块 `WXProvincialFreeCompany v0.3.3`

### 1. 原版数据确认

- 原版物品 `empire_lord_helmet` 的官方简中名称为“禁卫射手盔”，英文后备名为 `Palatine Guard Helmet`，提供 46 点头甲、重量 3.2。
- 原版 T5“帝国禁卫射手”三套装备此前全部使用 `roundkettle_over_imperial_mail`，仅有 34 点头甲。
- 原版 T5“帝国军士弩手”三套装备此前分别使用 36、42、34 点头甲的三种圆顶锅盔。
- 结论：用户记忆准确；与禁卫射手同名的华丽金色头盔确实没有装备给原版禁卫射手。

### 2. 北境肩甲分级

| 等级 | 新肩甲 | 身甲 | 臂甲 | 重量 | 设计含义 |
|---|---|---:|---:|---:|---|
| T3 两套 | `fur_cape_b` | 8 | 6 | 1.2 | 朴素诺德毛皮披肩，表现边境征召辅助兵 |
| T4 两套 | `nord_fur_shoulder` | 13 | 4 | 3.0 | 长亚麻毛皮披肩，表现正规化北境辅助军 |
| T5 方案 1、2 | `nord_fur_shoulder` | 13 | 4 | 3.0 | 延续统一军装与帝国重甲骨架 |
| T5 方案 3 | `wolf_shoulder_ulfhedinn` | 15 | 6 | 3.0 | 与斯特吉亚封闭盔组合为北境老兵变体 |

- T4/T5 旧帝国肩甲为身甲 6、臂甲 11、重量 3.9；长毛皮的身甲与臂甲总和同为 17，但把防护从手臂转移到躯干。
- 北境线继续保留帝国头盔、胸甲、手套和靴子，以肩甲、圆盾、战斧承担地域特征。
- 毛皮装备来自 NavalDLC，本版本开始将 NavalDLC `v1.2.8` 声明为必需依赖。

### 3. 帝国 T5 远程兵头盔覆盖

- 为 `imperial_palatine_guard` 和 `imperial_sergeant_crossbowman` 各加入一个局部 XML 覆盖节点。
- 完整保留原版三套装备中的远程武器、弹药、副武器、盾牌、胸甲、肩甲、手套、靴子与平民模板。
- 仅将六个战斗装备方案的 `Head` 槽统一替换为 `Item.empire_lord_helmet`。
- 禁卫射手头甲由 34 提高至 46；军士弩手三套方案分别提高 10、4、12 点头甲。
- 不修改两种原版兵种的技能、等级、升级路线、文化与战斗分组。

### 4. 静态校验

| 项目 | 结果 |
|---|---|
| `NPCCharacters.xsd` | 0 个错误 |
| 装备引用 | 79 个唯一物品 ID，缺失 0 |
| 北境肩甲方案 | T3 2 套、T4 2 套、T5 3 套全部匹配设计 |
| 其他自定义兵种数据 | 除北境 7 个 Cape 槽外，与 v0.3.2 完全一致 |
| 帝国 T5 远程兵非头盔装备 | 与原版逐槽一致 |
| 帝国 T5 远程兵头盔 | 6 套全部为 `empire_lord_helmet` |
| 模块依赖 | `NavalDLC v1.2.8` 已声明为必需依赖 |
| 重复 NPC ID | 0 |

### 5. 回滚点

- v0.3.2 安装副本：`C:\Users\WANGXINYU858\Documents\Codex\2026-10-11\new-chat\backups\WXProvincialFreeCompany-v0.3.2-provincial-levy`
- 开发分支：`feature/v0.3.3-northern-fur-ranged-helmets`

### 6. Git 发布与合并

- 本地功能提交：`38fb914`（`feat: implement v0.3.3 northern fur and ranged helmets`）。
- 远端功能提交：`72c0c89cecbd63971fca978abd7f7fd2820890c4`。
- Pull Request：[PR #3：v0.3.3 北境毛皮肩甲与帝国远程精锐军容](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/3)。
- PR 最终差异：7 个文件、190 行新增、19 行删除；只包含 v0.3.3 相关内容。
- 合并方式：Squash merge。
- `main` 合并提交：`9f310e81c61d3510b7ee94463fa0d52f98b7ba47`。
- 远端 `main` 的 7 个变更文件与本地 v0.3.3 Git Blob 逐项一致，差异 0。
- 本地 `main` 已快进至 `origin/main`，工作区干净；v0.3.3 功能分支暂时保留。
- 已安装模块保持 `v0.3.3`，源码与安装目录的 6 个部署文件哈希一致。

### 7. v0.3.3 手动验收项目

- [ ] Launcher 显示模块版本 `v0.3.3`，并在 NavalDLC 之后加载，无依赖警告。
- [ ] 北境 T3 两套装备均出现朴素毛皮披肩。
- [ ] 北境 T4 两套装备均出现长亚麻毛皮披肩。
- [ ] 北境 T5 两套长毛皮方案与一套狼皮方案均能随机出现。
- [ ] 毛皮肩甲与帝国头盔、军团甲、札甲之间没有明显穿模、悬空或领口遮挡。
- [x] 帝国禁卫射手和帝国军士弩手的头盔随机方案已检查；验收失败：v0.3.3 将金色头盔套装追加到原版装备池，原版头盔仍会随机出现，问题转入 v0.3.4 修复。
- [ ] 两种帝国 T5 远程兵的武器、弹药、副武器和盾牌仍与原版一致。
- [ ] 禁用 NavalDLC 时模块被正确阻止加载或给出依赖提示，而不是进入战斗后崩溃。

阶段说明：本轮不启动 Steam、Modding Kit、Launcher 或游戏；实机验收继续由用户手动完成。

---

## 战斗记录 015：v0.3.4 帝国远程兵头盔 XSLT 修复

**日期：** 2026-11-04
**目标：** 修复 v0.3.3 仅向帝国禁卫射手和帝国军士弩手追加金色头盔套装、未真正取代原版头盔的问题。
**环境：** Bannerlord `v1.4.8`、War Sails / NavalDLC `v1.2.8`、模块 `WXProvincialFreeCompany v0.3.4`

### 1. 用户验收发现

- v0.3.3 的北境毛皮肩甲等内容大体正常。
- 帝国禁卫射手与帝国军士弩手能够随机出现金色禁卫射手盔，但原版圆顶锅盔仍然存在。
- 这说明 v0.3.3 写入的三套装备并未覆盖原版三套方案，而是被游戏追加到了原版装备池。

### 2. 根因

- Bannerlord `v1.3+` 的托管 XML 合并会根据唯一属性识别同一个元素。
- `EquipmentRoster` 没有可供合并器逐套匹配的唯一 ID，因此局部 XML 中的三个装备套装被视为新元素。
- v0.3.3 的静态检查只能证明新增的六个 `Head` 槽写成了金色头盔，不能证明运行时合并后原版套装已被删除；实机验收暴露了这一检查盲区。
- 对原版兵种内部无唯一 ID 的嵌套列表进行精确修改，应使用 TaleWorlds 官方支持的 XSLT 转换，而不是继续追加同 ID 的局部兵种节点。

### 3. v0.3.4 实现

- 从 `spnpccharacters.xml` 删除 `imperial_palatine_guard` 与 `imperial_sergeant_crossbowman` 两个局部覆盖节点，避免继续追加六套金盔装备方案。
- 新增 `ModuleData/spnpccharacters.xslt`。
- XSLT 先以恒等规则原样复制全部原版 NPC 数据，再精确匹配以下节点：
  - `imperial_palatine_guard` 的所有战斗 `EquipmentRoster/equipment[@slot='Head']`；
  - `imperial_sergeant_crossbowman` 的所有战斗 `EquipmentRoster/equipment[@slot='Head']`。
- 六个目标头盔槽统一替换为 `Item.empire_lord_helmet`，即官方简中“禁卫射手盔”。
- 不修改原版模块文件，不增加 DLL 或 Harmony，不触碰两种兵的武器、弹药、盾牌、其他甲胄、技能、等级和升级关系。

### 4. 静态验证

| 项目 | 结果 |
|---|---|
| 模组 `spnpccharacters.xml` 解析 | 通过 |
| `spnpccharacters.xslt` 编译 | 通过 |
| 模组 XML 中两种原版兵的重复节点 | 0 个 |
| 帝国禁卫射手战斗装备数量 | 转换前 3 套，转换后仍为 3 套 |
| 帝国军士弩手战斗装备数量 | 转换前 3 套，转换后仍为 3 套 |
| 转换后头盔 | 6/6 均为 `Item.empire_lord_helmet` |
| 非头盔装备 | 转换前后逐槽完全一致 |
| 模组 XML 的 `NPCCharacters.xsd` | 通过 |
| 转换后原版 XML 的 `NPCCharacters.xsd` | 通过 |
| 仓库源码与安装副本 | 7 个文件哈希全部一致 |
| 已安装模块版本 | `v0.3.4` |

### 5. 分支与回滚

- 开发分支：`fix/v0.3.4-xslt-ranged-helmets`。
- v0.3.3 回滚基线：本地与远端 `main` 提交 `9f310e8`。
- 独立文件备份目录因本机权限策略未能创建，但 v0.3.3 的全部源码与文档已经由 Git 提交和 GitHub 远端双重保存。
- 回滚时可恢复 v0.3.3 的 `spnpccharacters.xml` 和 `SubModule.xml`，并删除 `spnpccharacters.xslt`。

### 6. v0.3.4 手动验收项目

- [ ] Launcher 显示模块版本 `v0.3.4`，并在 NavalDLC 之后加载，无 XML/XSLT 错误。
- [ ] 自定义战斗中多次刷新帝国禁卫射手，始终显示金色禁卫射手盔。
- [ ] 自定义战斗中多次刷新帝国军士弩手，始终显示金色禁卫射手盔。
- [ ] 两种兵仍只随机使用原版三套武器和防具组合，没有出现重复套装、裸体或缺失装备。
- [ ] 战役百科中两种兵稳定显示金色头盔。
- [ ] 已有测试存档能够正常加载，两种兵的技能、升级关系和其他装备没有异常。

### 7. Git 发布与合并

- 开发分支：`fix/v0.3.4-xslt-ranged-helmets`。
- 核心修复提交：`47111a5`（`fix: replace Imperial ranged helmets via XSLT in v0.3.4`）。
- 发布记录提交：`a8b4675`（`docs: record v0.3.4 pull request`）。
- Pull Request：[PR #4：v0.3.4 使用 XSLT 稳定替换帝国远程兵头盔](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/4)。
- PR 最终差异：7 个文件、115 行新增、92 行删除；GitHub 判定可合并，仓库未配置 CI 检查。
- 合并方式：Squash merge。
- `main` 合并提交：`55ebbef6388161174fe2c3a86900c2871e6414aa`。
- 合并时间：2026-11-04 21:10（Asia/Shanghai）。
- 本地 `main` 已快进至 `origin/main`，工作区干净；v0.3.4 修复分支暂时保留。

阶段说明：本轮只完成源码修改、XSLT 内存转换、Schema 校验和模块部署，没有启动 Steam、Modding Kit、Launcher 或游戏；实机验收继续由用户手动完成。

---

## 战斗记录 016：v0.3.5 南境标枪突击步兵线

**日期：** 2026-11-04
**目标：** 将南境 T3–T5 从通用占位配置改造成受阿塞莱战法影响、但仍具有帝国军制身份的双手兵器与标枪突击步兵。
**环境：** Bannerlord `v1.4.8`、War Sails / NavalDLC `v1.2.8`、模块 `WXProvincialFreeCompany v0.3.5`

### 1. 设计结论

- 正式路线：`T3 南境行省掷矛兵 → T4 南境自由军突击兵 → T5 南境自由军破阵卫`。
- 战术循环：接敌前投出一组标枪，随后切换双手斧或双手锤发起冲击。
- 全线保持 `default_group="Infantry"`，不设为远程分组；全线不装备盾牌。
- 每套严格只配置一组标枪，防止 AI 长时间停留在投掷阶段，也避免兵种退化为无盾纯远程单位。
- 帝国身份由胸甲、多数头盔、靴子和护手承担；南方身份由塔巴尔斧、行刑斧、Jereed、披巾、皮护肩和青铜鳞片肩甲承担。
- 不添加 `Mariner` 技能；本版本先建立陆战身份，海军陆战方向留待将来单独验证。

### 2. 当前版本的阿塞莱参照

- War Sails 保留了原有马穆鲁克双手步兵与法里斯标枪骑兵路线，另增 T4 阿塞莱老练水手和 T5 阿塞莱巴赫里耶水兵。
- 新增水兵主要使用弓、盾和单手剑，因此南境“双手兵器＋标枪”的设计不会与 DLC 海军兵种重合。
- 原版 T5 阿塞莱宫廷卫士的跑动、双手、投掷分别为 140、140、130；南境 T5 控制为 135、135、125，并保持无盾与较弱的一套黄铜盔方案，避免普通兵线压过阿塞莱精锐。

### 3. 装备与视觉分级

| 等级 | 装备方案 | 标枪 | 帝国骨架 | 南方特征 |
|---|---|---|---|---|
| T3 | 2 套短柄塔巴尔斧 | 简易标枪，各 1 组 | 帝国软甲、帝国鼻护盔/软帽 | 皮质南方护肩或披巾 |
| T4 | 1 套行刑斧、1 套南方双手锤 | Jereed，各 1 组 | 帝国锁甲、帝国头盔 | 青铜鳞片护肩 |
| T5 | 2 套重型行刑斧、1 套南方双手锤 | Jereed，各 1 组 | 三套帝国军团甲、两套帝国重盔 | 装饰青铜鳞片肩甲；一套阿塞莱黄铜盔 |

- T5 新增第三套装备不是额外合并原版兵种，而是自定义兵种自身的合法随机方案。
- T5 黄铜盔仅占三套之一，控制南方视觉比例；胸甲仍全部为帝国军团甲。

### 4. 技能分配

| 等级 | 跑动 | 双手 | 投掷 | 单手 | 长杆 | 设计含义 |
|---|---:|---:|---:|---:|---:|---|
| T3 | 75 | 80 | 70 | 25 | 40 | 从无组织 T2 役卒转为正规掷矛兵 |
| T4 | 105 | 110 | 100 | 30 | 50 | 形成稳定的投枪后双手冲击能力 |
| T5 | 135 | 135 | 125 | 40 | 60 | 高爆发破阵兵，但略低于宫廷卫士 |

- 弓、弩、骑术仍按同阶通用背景值保留，不承担实战定位。
- 北境线负责持盾与持续作战；南境线负责投掷爆发和近战破阵，两者形成明确互补。

### 5. 静态验证

| 项目 | 结果 |
|---|---|
| `NPCCharacters.xsd` | 通过 |
| 南境 T3/T4/T5 装备方案 | 2 / 2 / 3 套 |
| 战斗分组 | 3 个兵种均为 `Infantry` |
| 盾牌 | 0 |
| 标枪 | 7 套装备各 1 组，共 7 组 |
| 重复 NPC ID | 0 |
| 装备引用 | 80 个唯一物品 ID，缺失 0 |
| 非南境自定义兵种 | 与 v0.3.4 完全一致 |
| 模块版本 | `v0.3.5` |
| 仓库源码与安装副本 | 7 个文件哈希全部一致 |

### 6. v0.3.5 手动验收项目

- [ ] Launcher 显示模块版本 `v0.3.5`，在 NavalDLC 之后加载且无 XML 错误。
- [ ] 百科与自定义战斗中的名称依次为“南境行省掷矛兵”“南境自由军突击兵”“南境自由军破阵卫”。
- [ ] T3 两套装备均为短柄塔巴尔斧＋一组简易标枪，没有盾牌或单手副武器。
- [ ] T4 能随机出现行刑斧和南方双手锤，两套均使用一组 Jereed 与青铜鳞片肩甲。
- [ ] T5 能随机出现两套重型行刑斧与一套双手锤，其中两套帝国重盔、一套阿塞莱黄铜盔。
- [ ] AI 会在接敌前投掷标枪，并能可靠切换到双手武器冲锋，不会因弹药有限而停滞。
- [ ] 南境线对同阶步兵具有明显爆发杀伤，但无盾状态下抗箭、生存和持线能力低于北境线。
- [ ] 共同 T2 仍可正常升级至南境 T3，南境 T3、T4 后续升级关系正常。
- [ ] 北境、东境、西境以及帝国禁卫射手/军士弩手没有出现回归问题。

### 7. Git 发布与合并

- 开发分支：`feature/v0.3.5-southern-line`。
- 核心功能提交：`f95e817`（`feat: implement v0.3.5 southern javelin line`）。
- 发布记录提交：`261cb3f`（`docs: record v0.3.5 pull request`）。
- Pull Request：[PR #5：v0.3.5 南境标枪突击步兵线](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/5)。
- PR 最终差异：7 个文件、156 行新增、57 行删除；GitHub 判定可合并，仓库未配置 CI 检查。
- 合并方式：Squash merge。
- `main` 合并提交：`740712bdf559b083754d438cd9edd93cfde56410`。
- 合并时间：2026-11-04 21:31（Asia/Shanghai）。
- 本地 `main` 已快进至 `origin/main`，工作区干净；v0.3.5 功能分支暂时保留。

阶段说明：本轮不启动 Steam、Modding Kit、Launcher 或游戏；实机验收继续由用户手动完成。

---

## 战斗记录 017：v0.3.6 西境无盾重弩兵线

**日期：** 2026-11-04
**目标：** 将西境 T3–T5 从通用占位兵种改造为受瓦兰迪亚制弩技术影响、但仍保留帝国军制与甲胄主体的专业重弩路线。
**环境：** Bannerlord `v1.4.8`、War Sails / NavalDLC `v1.2.8`、模块 `WXProvincialFreeCompany v0.3.6`

### 1. 设计结论

- 正式路线：`T3 西境行省弩手 → T4 西境自由军重弩手 → T5 西境自由军破甲弩卫`。
- 三个兵种统一改为 `default_group="Ranged"`，全线不装备盾牌。
- T3 携带一组弩矢；T4、T5 每套装备携带两组弩矢，用火力续航形成核心特色。
- 全线使用双手长矛作为近敌或骑兵突入时的有限自卫手段，不配置高熟练度的单手剑盾组合。
- 帝国身份由软甲、锁甲、军团甲、帝国鼻护盔和精钢长矛承担；西境影响由弩、兜帽、西式皮肩甲和少量壶盔承担。
- 不使用瓦兰迪亚阔盾，避免复制瓦兰迪亚神射手；不配置帝国军士弩手式的盾牌与高单手熟练度，避免定位冲突。

### 2. 装备与火力分级

| 等级 | 远程武器 | 弹药 | 近战副武器 | 防具与地域视觉 |
|---|---|---|---|---|
| T3 | 轻弩 `crossbow_e` | 1 组轻弩矢 `bolt_e`，18 发 | 两种瓦兰迪亚长矛 | 帝国软甲为主；一套帝国皮衬壶盔＋兜帽，一套帝国鼻护盔＋披巾 |
| T4 | 重弩 `crossbow_b` | 2 组普通弩矢 `bolt_b`，共 36 发 | 两种帝国精钢长矛 | 帝国锁甲为主；加入西式皮肩甲与壶盔变体 |
| T5 | 缠弦弩 `crossbow_f` | 2 组重弩矢 `bolt_c`，共 36 发 | 两种帝国精钢长矛 | 帝国军团甲为主；一套帝国重鼻护盔，一套瓦兰迪亚锁甲壶盔＋兜帽 |

- 原版当前数据中，轻弩伤害/弹速/精度为 77/65/85，重弩为 82/80/96，缠弦弩为 100/97/100，形成明确的三级火力成长。
- T4 与 T5 的四个武器槽全部用于“弩＋两组弹药＋长矛”，因此没有空间再携带盾牌或单手武器。

### 3. 技能分配

| 等级 | 跑动 | 弩 | 长杆 | 单手 | 设计含义 |
|---|---:|---:|---:|---:|---|
| T3 | 70 | 80 | 50 | 25 | 初步掌握轻弩，以长矛勉强自卫 |
| T4 | 100 | 110 | 80 | 40 | 成为弹药充足的专业重弩手 |
| T5 | 125 | 140 | 100 | 50 | 专职破甲射手，但近战明显偏科 |

- 原版 T5 帝国军士弩手为弩 130、单手 130、跑动 130，并携带盾牌；西境 T5 的弩术略高，但单手、跑动与自保能力明显更低。
- 原版 T5 瓦兰迪亚神射手为弩 130、单手 130，并携带阔盾；西境 T5 依靠双份弹药而非盾牌获得差异化优势。

### 4. 静态验证

| 项目 | 结果 |
|---|---|
| 模组 `spnpccharacters.xml` 解析与 `NPCCharacters.xsd` | 通过，0 个错误 |
| `spnpccharacters.xslt` 编译与转换后 Schema | 通过 |
| 帝国两种 T5 远程兵 XSLT 头盔槽 | 6/6 仍为 `Item.empire_lord_helmet` |
| 重复 NPC ID | 0 |
| 装备引用 | 91 个唯一物品 ID，缺失 0 |
| 西境战斗分组 | T3/T4/T5 均为 `Ranged` |
| 西境盾牌 | 0 |
| 每套弩矢 | T3 一组，T4/T5 两组 |
| 非西境自定义兵种节点 | 与 v0.3.5 完全一致 |
| 模块版本 | `v0.3.6` |

### 5. v0.3.6 手动验收项目

- [ ] Launcher 显示模块版本 `v0.3.6`，在 NavalDLC 之后加载且无 XML/XSLT 错误。
- [ ] 百科与自定义战斗中的名称依次为“西境行省弩手”“西境自由军重弩手”“西境自由军破甲弩卫”。
- [ ] 三个兵种均进入远程阵型，不再作为普通步兵主动冲锋。
- [ ] T3 两套装备均为轻弩＋一组轻弩矢＋长矛，且没有盾牌。
- [ ] T4 两套装备均为重弩＋两组普通弩矢＋精钢长矛，双份弹药可以正常消耗。
- [ ] T5 两套装备均为缠弦弩＋两组重弩矢＋精钢长矛，并能稳定表现出最高远程杀伤。
- [ ] AI 在敌人或骑兵逼近后能够从弩切换到长矛，不会因无单手武器或重复弹药槽而停滞。
- [ ] 与同数量帝国军士弩手比较时，西境 T5 火力续航更长，但近战、自保和抗远程能力更弱。
- [ ] 与瓦兰迪亚神射手比较时，西境 T5 不会因弩术 140 和双份重弩矢形成无代价的全面上位替代。
- [ ] 北境、南境、东境、共同 T2 以及帝国禁卫射手/军士弩手没有出现回归问题。

### 6. Git 发布与合并

- 开发分支：`feature/v0.3.6-western-crossbow-line`。
- 核心功能提交：`dbdb25d`（`feat: implement v0.3.6 western crossbow line`）。
- 发布记录提交：`d17549a`（`docs: record v0.3.6 western crossbow release`）。
- Pull Request：[PR #6：v0.3.6 西境无盾重弩兵线](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/6)。
- PR 最终差异：8 个文件、165 行新增、70 行删除；GitHub 判定 `CLEAN / MERGEABLE`，仓库未配置 CI 检查。
- 合并方式：Squash merge。
- `main` 合并提交：`bf967150803b02d28bbc4df5c5a5f49caeeb9284`。
- 合并时间：2026-11-04 21:53（Asia/Shanghai）。
- 本地 `main` 已快进至 `origin/main`，工作区干净；v0.3.6 功能分支暂时保留。

阶段说明：本轮只修改、校验和部署模块文件，不启动 Steam、Modding Kit、Launcher 或游戏；实机验收继续由用户手动完成。

---

## 战斗记录 018：v0.3.7 东境骑射游骑兵线

**日期：** 2026-11-04
**目标：** 将最后一条东境 T3–T5 占位路线改造为受库赛特骑射战法影响、但仍服从帝国行省军制的游骑兵。
**环境：** Bannerlord `v1.4.8`、War Sails / NavalDLC `v1.2.8`、模块 `WXProvincialFreeCompany v0.3.7`

### 1. 设计结论

- 正式路线：`T3 东境行省骑射手 → T4 东境自由军游骑射手 → T5 东境自由军游骑卫`。
- 三个兵种统一改为 `default_group="HorseArcher"`，全线不装备盾牌。
- 每套武器严格固定为“一张骑射弓＋一袋草原箭＋一支东方长矛＋一把帝国剑”。
- 每套只有 24 支箭，不使用库赛特普通高阶骑射手的双箭袋；设计循环为骑射骚扰、箭尽后长矛侧击、最后用帝国剑近战。
- 不使用长偃月刀、盾牌和重型骑枪，防止同时获得持续骑射、强近战和重骑冲锋三重优势。
- 帝国身份由软甲、锁甲、军团甲、帝国头盔和帝国剑承担；东境影响由骑射弓、东方长矛、草原头肩装备、马匹与马具承担。

### 2. 弓马与装备分级

| 等级 | 弓箭 | 长矛与副剑 | 马匹与马具 | 防具与地域视觉 |
|---|---|---|---|---|
| T3 | 草原弓＋1 袋草原箭 | 加重草原矛＋T3 帝国剑 | 普通草原马＋草原鞍 | 帝国骑手软甲或帝国软甲；帝国锅盔/草原毛皮兜帽 |
| T4 | 重型反曲弓＋1 袋草原箭 | 加重草原矛＋T3/T4 帝国剑 | 草原猎马＋札甲半身马具 | 帝国锁甲；一套帝国鼻护盔、一套草原尖顶盔 |
| T5 | 草原反曲弓＋1 袋草原箭 | 精钢叶形矛＋T4/T5 帝国剑 | 草原猎马＋札甲半身马具 | 帝国军团甲；一套帝国重鼻护盔、一套羽饰游牧盔 |

- 草原弓伤害/弹速/精度为 41/68/80，重型反曲弓为 52/76/91，草原反曲弓为 52/78/94。
- 草原箭每袋 24 支，三个等级均保持单箭袋，以技能、弓和马匹升级形成成长，而不是增加持续绕射时间。
- T3 草原马为速度 45、操纵 67、额外生命 0，草原鞍提供 12 马甲；T4–T5 草原猎马为速度 47、操纵 69、额外生命 60，半身札甲提供 38 马甲。

### 3. 技能分配与原版对照

| 等级 | 骑术 | 弓 | 长杆 | 单手 | 跑动 | 设计含义 |
|---|---:|---:|---:|---:|---:|---|
| T3 | 70 | 65 | 60 | 45 | 40 | 能完成基础骑射，但略低于同级库赛特坎克利 |
| T4 | 100 | 95 | 80 | 65 | 55 | 专业游骑射手，长矛能力高于库赛特普通骑射手 |
| T5 | 130 | 125 | 105 | 90 | 70 | 可靠骑射与侧击精锐，但近战和续航受限 |

- 原版 T5 库赛特重装骑射手为骑术 130、弓 130、单手 130、长杆 80，并携带两袋箭。
- 东境 T5 保持相同骑术，弓低 5、单手低 40，只携带一袋箭；长杆高 25，用轻矛侧击形成自己的优势。
- 跑动只提升至 70，意味着海战、攻城或强制下马环境是明确弱点，需要实机确认不会弱到失去基本作战能力。

### 4. 静态验证

| 项目 | 结果 |
|---|---|
| 模组 `spnpccharacters.xml` 解析与 `NPCCharacters.xsd` | 通过，0 个错误 |
| `spnpccharacters.xslt` 编译与转换后 Schema | 通过，0 个错误 |
| 帝国两种 T5 远程兵 XSLT 头盔槽 | 6/6 仍为 `Item.empire_lord_helmet` |
| 重复 NPC ID | 0 |
| 装备引用 | 102 个唯一物品 ID，缺失 0 |
| 东境战斗分组 | T3/T4/T5 均为 `HorseArcher` |
| 东境箭袋与盾牌 | 每套 1 袋草原箭，盾牌 0 |
| 东境马匹与马具 | 三阶均存在；T3 普通马，T4/T5 战马 |
| 非东境自定义兵种节点 | 与 v0.3.6 完全一致 |
| 模块版本 | `v0.3.7` |

### 5. v0.3.7 手动验收项目

- [ ] Launcher 显示模块版本 `v0.3.7`，在 NavalDLC 之后加载且无 XML/XSLT 错误。
- [ ] 百科与自定义战斗中的名称依次为“东境行省骑射手”“东境自由军游骑射手”“东境自由军游骑卫”。
- [ ] 三个兵种均有马匹与马具，并进入骑射阵型而不是普通步兵、远程或冲击骑兵阵型。
- [ ] 每套装备均为一张弓、一袋 24 支草原箭、一支东方长矛和一把帝国剑，没有盾牌或第二袋箭。
- [ ] T3 使用普通草原马，T4/T5 使用草原猎马和札甲半身马具，百科与战斗模型均正确显示。
- [ ] AI 能进行正常骑射绕行，箭尽、敌人贴近或追击溃兵时能够切换长矛或帝国剑，不会持续空弓绕行。
- [ ] 从共同 T2 升级东境 T3、从 T3 升级 T4 时，普通马、战马需求、升级费用和界面提示符合原版机制。
- [ ] 与同数量库赛特重装骑射手比较时，东境 T5 的远程续航和近战能力更弱，但长矛侧击具有可辨识价值。
- [ ] 在攻城、海战或强制下马测试中仍能完成基本作战，但明显弱于陆地开阔环境。
- [ ] 北境、南境、西境、共同 T2 以及帝国禁卫射手/军士弩手没有出现回归问题。

### 6. Git 发布与合并

- 开发分支：`feature/v0.3.7-eastern-horse-archer-line`。
- 核心功能提交：`d99ae7f`（`feat: implement v0.3.7 eastern horse archer line`）。
- 发布记录提交：`a77049f`（`docs: record v0.3.7 eastern horse archer release`）。
- Pull Request：[PR #7：v0.3.7 东境骑射游骑兵线](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/7)。
- PR 最终差异：8 个文件、183 行新增、78 行删除；GitHub 判定 `CLEAN / MERGEABLE`，仓库未配置 CI 检查。
- 合并方式：Squash merge。
- `main` 合并提交：`64bfe5e4a90a7e7745de749efbaf1431654912c9`。
- 合并时间：2026-11-04 22:12（Asia/Shanghai）。
- 本地 `main` 已快进至 `origin/main`，工作区干净；v0.3.7 功能分支暂时保留。

阶段说明：本轮只修改、校验和部署模块文件，不启动 Steam、Modding Kit、Launcher 或游戏；实机验收继续由用户手动完成。

---

## 战斗记录 019：v0.3.8 西境近战与东境骑射平衡修复

**日期：** 2026-11-05
**目标：** 根据四条地域路线的首轮实机反馈，修复西境弩兵贴身战斗过弱、东境骑射兵被库赛特同级单位全面压制，以及东境马具缺乏帝国辨识度的问题。
**环境：** Bannerlord `v1.4.8`、War Sails / NavalDLC `v1.2.8`、模块 `WXProvincialFreeCompany v0.3.8`

### 1. 实测结论与版本边界

- 北境路线已达到预期，本版本不修改。
- 南境路线实测明显强于阿塞莱同级步兵；用户认可当前高爆发表现，本版本不削弱，未来通过招募稀有度与培养成本处理系统性平衡。
- 西境失败点不是弩术，而是无盾弩兵被贴身后只能使用纯刺击长矛，密集近战中无法有效自保。
- 东境同时承受半数箭量、较低弓术、较低单手与库赛特马具同质化，弱点叠加超过设计预期。
- 本版本只修复已实测的西境与东境战斗问题，不改变 `imperial_recruit → wx_provincial_levy` 入口和 AI 升级权重；招募生态留待 v0.4.0。

### 2. 西境近战修复

| 等级 | v0.3.7 近战配置 | v0.3.8 近战配置 | 单手变化 |
|---|---|---|---:|
| T3 西境行省弩手 | 两种西式长矛 | 两套均为瓦兰迪亚低阶剑 | 25 → 70 |
| T4 西境自由军重弩手 | 两种帝国精钢长矛 | 两套均为瓦兰迪亚军用剑 | 40 → 95 |
| T5 西境自由军破甲弩卫 | 两种帝国精钢长矛 | 一套高阶瓦兰迪亚剑、一套瓦兰迪亚长柄偃月武器 | 50 → 115 |

- T3/T4 长杆调整为 40/60；T5 长杆 100 保留，用于长柄偃月武器方案。
- T4/T5 仍携带双份弩矢，四个武器槽仍为“弩＋弩矢＋弩矢＋近战武器”。
- 全线仍无盾，近战修复只让其能够自保，不赋予帝国军士弩手或瓦兰迪亚神射手的持盾能力。

### 3. 东境骑射修复

| 等级 | 弓术 | 单手 | 长杆 | 弓箭 | 帝国马具 |
|---|---:|---:|---:|---|---|
| T3 东境行省骑射手 | 65 → 70 | 45 → 60 | 60 → 70 | 草原弓改为反曲弓；24 支草原箭 | 条纹皮马具，15 马甲 |
| T4 东境自由军游骑射手 | 95 → 100 | 65 → 90 | 80 → 90 | 重型反曲弓；24 → 36 支箭 | 具装半身鳞甲，60 马甲 |
| T5 东境自由军游骑卫 | 125 → 130 | 90 → 115 | 105 → 110 | 草原反曲弓；24 → 36 支箭 | 具装半身鳞甲，60 马甲 |

- 库赛特 T4/T5 同级骑射手仍携带两袋共 48 支箭；东境继续只有一个箭袋和 36 支箭，远程续航仍低 25%。
- 草原马和草原猎马继续保留，保持机动来源与东境血统；只替换马具，形成“草原马匹＋帝国防护”的混合身份。
- 高阶马甲由库赛特札甲半身马具的 38 提高到帝国具装半身鳞甲的 60，改善坐骑生存率，但仍不配置重型骑枪、盾牌或双箭袋。

### 4. 静态验证

| 项目 | 结果 |
|---|---|
| 模组 XML 解析与 `NPCCharacters.xsd` | 通过，0 个错误 |
| `spnpccharacters.xslt` 编译与转换后 Schema | 通过，0 个错误 |
| 帝国两种 T5 远程兵 XSLT 头盔槽 | 6/6 仍为 `Item.empire_lord_helmet` |
| 重复 NPC ID | 0 |
| 装备引用 | 103 个唯一物品 ID，缺失 0 |
| 修改范围 | 仅西境 T3–T5、东境 T3–T5 与版本文档 |
| 未修改节点 | 北境、南境、共同 T2、原版兵种和升级入口与 v0.3.7 一致 |
| 模块版本 | `v0.3.8` |

### 5. v0.3.8 手动验收项目

- [ ] Launcher 显示模块版本 `v0.3.8`，在 NavalDLC 之后加载且无 XML/XSLT 错误。
- [ ] 西境 T3/T4 被贴身后能可靠切换单手剑，近战不再因长矛最小距离而完全失效。
- [ ] 西境 T5 能随机出现高阶剑和长柄偃月武器两种方案；偃月武器方案不会出现拒绝切换或原地停滞。
- [ ] 西境 T4/T5 仍各自携带两组弩矢，远程续航没有因近战修复回退。
- [ ] 东境 T3 正确显示反曲弓与帝国条纹皮马具；T4/T5 正确显示单袋 36 支箭和帝国半身鳞甲。
- [ ] 东境 T4/T5 对同级库赛特骑射手的战损差距明显缩小，但仍因 36 对 48 的箭量差距保留续航劣势。
- [ ] 东境箭尽后能够使用东方长矛和帝国剑参与侧击、追击与自卫。
- [ ] 帝国半身鳞甲没有严重拖慢绕射、转向或阵型跟随，也没有明显穿模。
- [ ] 北境、南境、共同 T2、升级树和两种原版帝国 T5 远程兵没有回归问题。

### 6. Git 发布与合并

- 开发分支：`fix/v0.3.8-western-eastern-balance`。
- 核心修复提交：`3f8fc83`（`fix: rebalance western and eastern lines in v0.3.8`）。
- 发布记录提交：`a9d36f1`（`docs: record v0.3.8 balance feedback and validation`）。
- Pull Request：[PR #8：v0.3.8 西境近战与东境骑射平衡修复](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/8)。
- PR 最终差异：7 个文件、138 行新增、40 行删除；GitHub 判定 `CLEAN / MERGEABLE`，仓库未配置 CI 检查。
- 合并方式：Squash merge。
- `main` 合并提交：`534f5d0433b4e53ba149a8f515c42b2e04bbda39`。
- 合并时间：2026-11-05 21:31（Asia/Shanghai）。
- 本地 `main` 已快进至 `origin/main`，工作区干净；v0.3.8 修复分支暂时保留。

阶段说明：本轮只修改、校验和部署模块文件，不启动 Steam、Modding Kit、Launcher 或游戏；实机验收继续由用户手动完成。

---

<a id="combat-campaign-migration"></a>

## 战役设计迁移说明

原记录020“v0.3.8 对战役生态的全局影响评估”和记录021“v0.4 地域独立兵源与边疆梯度招募方案”已完整迁移至：

[`骑砍2兵种扩展MOD开发日志-2战役设计部分.md`](./骑砍2兵种扩展MOD开发日志-2战役设计部分.md)

记录009、013、022、023同时包含兵种本体和战役信息，为保留版本实现上下文仍留在本册；其中与征募、费用、AI生态和存档迁移有关的结论已汇总到第二册。后续战役层的新调查与实现只写入第二册。

---

<a id="combat-low-tier"></a>

## 战斗记录 020（原全局记录 022）：v0.4.0 四套地域边民与行省征召兵数据层

**日期：** 2026-11-05

**目标：** 实现 v0.4 第一步兵种数据层，新增四种地域边民 T1 和四种地域行省征召兵 T2，并分别单线接入现有四条 T3–T5。

**环境：** Bannerlord `v1.4.8`、War Sails / NavalDLC `v1.2.8`、模块 `WXProvincialFreeCompany v0.4.0`

### 1. 本轮范围

本轮只实现兵种对象、升级关系、装备、技能、本地化、版本文档和模块部署，不提前实现 v0.4 第二步的 C# 地域志愿兵模型。

为隔离兵种数据测试与战役招募生态重构：

- 原版 `imperial_recruit` 到旧共同 T2 `wx_provincial_levy` 的追加入口暂时保留。
- 旧共同 T2 及其四个 T3 目标完整保留。
- 新增的四种 T1 暂无正式战役志愿兵来源，先用于百科、自定义战斗、作弊测试和升级树验证。
- 不修改 `kingdom_hero_party_empire_template`，帝国领主重建模板仍不直接发放 MOD 兵。
- 不添加 DLL、Harmony 或持续运行的战役逻辑。

### 2. 新增兵种与升级链

| 地域 | T1 ID / 中文名 | T2 ID / 中文名 | 现有 T3 目标 |
|---|---|---|---|
| 北境 | `wx_northern_borderlander` / 北境边民 | `wx_northern_provincial_levy` / 北境行省征召兵 | `wx_northern_provincial_auxiliary` / 北境行省辅兵 |
| 南境 | `wx_southern_borderlander` / 南境边民 | `wx_southern_provincial_levy` / 南境行省征召兵 | `wx_southern_provincial_auxiliary` / 南境行省掷矛兵 |
| 西境 | `wx_western_borderlander` / 西境边民 | `wx_western_provincial_levy` / 西境行省征召兵 | `wx_western_provincial_auxiliary` / 西境行省弩手 |
| 东境 | `wx_eastern_borderlander` / 东境边民 | `wx_eastern_provincial_levy` / 东境行省征召兵 | `wx_eastern_provincial_auxiliary` / 东境行省骑射手 |

升级关系：

```text
北境边民 T1 → 北境行省征召兵 T2 → 北境行省辅兵 T3 → 北境自由军盾斧兵 T4 → 北境自由军盾卫 T5
南境边民 T1 → 南境行省征召兵 T2 → 南境行省掷矛兵 T3 → 南境自由军突击兵 T4 → 南境自由军破阵卫 T5
西境边民 T1 → 西境行省征召兵 T2 → 西境行省弩手 T3 → 西境自由军重弩手 T4 → 西境自由军破甲弩卫 T5
东境边民 T1 → 东境行省征召兵 T2 → 东境行省骑射手 T3 → 东境自由军游骑射手 T4 → 东境自由军游骑卫 T5
```

每个新增 T1 只有一个同地域 T2 目标；每个新增 T2 只有一个同地域 T3 目标。地域锁定由兵种 ID 和线性升级树本身完成。

### 3. T1“边民”共同设计

四种 T1 均为等级 6、帝国文化、原版 T1 技能档次，表现尚未接受正式军事训练的当地居民。

| 兵种 | 战斗分组 | 主要技能 | 装备与地域暗示 |
|---|---|---|---|
| 北境边民 | Infantry | 跑动 25、单手 20、长杆 20 | 民用锤/草叉、帝国布衣、毛皮兜帽或围巾 |
| 南境边民 | Infantry | 跑动 25、双手 20、投掷 15 | 镰刀/民用锤、石块、帝国布衣、南方缠巾 |
| 西境边民 | Infantry | 跑动 20、单手 20、长杆 20、弩 15 | 民用锤/草叉、帝国布衣、兜帽和西式便鞋 |
| 东境边民 | Ranged | 弓 20、长杆 20、骑术 10 | 猎弓、普通箭、农用长杆、草原软帽/兜帽；无马 |

东境 T1 的两套战斗装备均包含猎弓与箭，避免被编入远程阵型后出现无远程武器的装备方案。

### 4. T2“行省征召兵”地域专业化

四种 T2 均为等级 11，主技能控制在 40–50 左右，与原版帝国 T2 档位接近；它们开始获得帝国软甲与地方训练，但尚未达到现有 T3 的正式辅助兵水平。

| 兵种 | 分组 | 核心技能 | 武器与防具 | 向 T3 的过渡 |
|---|---|---|---|---|
| 北境行省征召兵 | Infantry | 单手 45、跑动 45、投掷 25 | T2 北方单手斧、简陋圆盾、帝国软甲 | T3 换装更好的北方斧、圆盾与标枪 |
| 南境行省征召兵 | Infantry | 双手 50、投掷 40、跑动 50 | 简易双手长斧、单组低阶标枪、帝国软甲与南方披肩 | T3 获得塔巴尔斧和更成熟的掷矛训练 |
| 西境行省征召兵 | Ranged | 弩 45、单手 35、跑动 40 | 农兵弩、单组普通弩矢、低阶瓦兰迪亚剑、无盾 | T3 换装轻弩并提高弩术与近战能力 |
| 东境行省征召兵 | Ranged | 弓 45、长杆 35、骑术 30、跑动 35 | 猎弓、普通箭、东方短矛、帝国软甲、无马 | T3 才获得反曲弓、草原箭、帝国剑和坐骑并转入 HorseArcher |

东境 T1/T2 没有 `Horse` 或 `HorseHarness` 槽位，防止 T2 提前成为低阶骑射兵；坐骑门槛继续保留在 T3。

### 5. 兼容与迁移状态

- 现有十二个 T3–T5 ID、装备、技能和中文名称均未重命名。
- 旧共同 T2 `wx_provincial_levy` 未删除，旧存档中的现有单位仍可四选一升级。
- 原版帝国新兵追加入口暂未解除，所以 v0.4.0 数据层不会立刻改变现有战役兵源。
- v0.4 第二步完成地域志愿兵模型后，才会解除旧入口并由聚落配置生成四种地域 T1。
- 默认帝国领主模板、民兵、叛军模板、聚落数据和原版文件均未修改。

### 6. 静态校验结果

- `NPCCharacters.xsd`：0 个错误。
- 自定义 NPC：22 个唯一 ID，0 个重复。
- 新增升级关系：8/8 与设计一致，0 个缺失目标。
- 装备引用：114 个唯一 ID，0 个缺失。
- 简体中文本地化：21 个字符串，0 个缺失。
- 东境 T1/T2：全部战斗装备方案均有弓，0 个马匹或马具槽位。
- 原版远程兵 XSLT 内存转换：6/6 个战斗头盔仍为 `Item.empire_lord_helmet`。
- XSLT 转换后的原版 `spnpccharacters` 再次通过 Schema，0 个错误。
- `SubModule.xml` 版本已更新为 `v0.4.0`。

### 7. 部署结果

- 已将源码模块复制到游戏安装目录 `Modules\WXProvincialFreeCompany`。
- 源码与安装目录逐文件比较：7 个文件，0 个缺失，0 个 SHA-256 差异。
- 安装目录 `SubModule.xml` 显示 `v0.4.0`。
- 本轮没有启动 Steam、Modding Kit、Launcher 或游戏。

### 8. 手动验收项目

- [ ] Launcher 显示模块版本 `v0.4.0`，加载顺序和依赖无警告。
- [ ] 自定义战斗或百科能够看到八个新 T1/T2，中文名称、等级和分组正确。
- [ ] 四种边民只能升级为同地域征召兵，四种征召兵只能升级为同地域 T3。
- [ ] 北境 T2 正确使用单手斧与圆盾，能够正常格挡、接敌和攻击。
- [ ] 南境 T2 能先投出一组标枪，再切换双手长斧近战。
- [ ] 西境 T2 被编入远程阵型，能使用弩射击并在近敌时切换单手剑。
- [ ] 东境 T1/T2 均被编入远程阵型、没有马匹，能使用弓箭并在近敌时切换长杆；T3 才变为骑射兵。
- [ ] 新 T1/T2 与原版帝国同阶单位对比时没有明显全面上位表现。
- [ ] 旧共同 T2、现有四条 T3–T5 和两种原版帝国 T5 金盔没有回归问题。

### 9. Git 发布与合并

- 开发分支：`feature/v0.4.0-regional-recruit-tiers`。
- 核心功能提交：`5a84fa2154f176ce6f9a4d0381c9b961f877c529`（`feat: add regional borderlander and levy tiers for v0.4.0`）。
- 发布记录提交：`e2533ebf0ac887d5bd555a1c7909fdfeb26fe84f`、`b3c80425b1b82142ab9401d69e8e33bdf2151b97`。
- Pull Request：[PR #9：v0.4.0 新增四套地域边民与行省征召兵](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/9)。
- PR 最终差异：8 个文件、906 行新增、15 行删除。
- GitHub 判定：`CLEAN / MERGEABLE`；仓库未配置 CI 检查。
- 2026-11-05 22:42（Asia/Shanghai）以 squash merge 合并到 `main`；合并提交：`792af4b6c5e922efbf658e168e9d4d0019408fe2`。
- 本地 `main`、`origin/main` 已同步到上述提交，工作区干净；安装目录与仓库源码 7 个文件的 SHA-256 对比为 0 项差异。

---

## 战斗记录 021（原全局记录 023）：v0.4.1 地域低阶兵视觉身份重构

**日期：** 2026-11-05
**目标：** 解决四种 T1 边民与原版帝国新兵、四种 T2 征召兵与原版帝国步兵及彼此之间辨识度不足的问题
**版本：** `v0.4.1-regional-visual-identity`
**开发分支：** `feature/v0.4.1-regional-visual-identity`

### 1. 设计原则

本轮采用“地方居民逐步帝国化”的装备成长逻辑：

- T1 以地域民用服饰、农具和生活武器为主，地域元素约占视觉主体的 80%–90%。
- T2 获得帝国软甲，但每套装备必须保留稳定出现的地域头肩、鞋靴或武器轮廓。
- T3–T5 继续作为已经完成军事整编的行省辅助军，本轮不修改。
- 使用外国文化的民用布衣不等于把边民改成外国正规军；避免完整复制原版外国兵种的整套军服和高级军械。

### 2. T1 地域边民重构

#### 北境边民

- 两套均固定使用 `tundra_tunic` 冻原束腰衣、`fur_hood` 毛皮兜帽、`highland_leg_wrappings` 高地裹腿和 `ragged_armwraps` 破旧绑臂。
- 武器分别为低阶农用短斧和农用锤；一套额外使用围巾。
- 移除原先与帝国新兵高度重叠的草叉、皮帽、步卒束腰衣、带护肩束腰衣和帝国骑手靴组合。
- 视觉定位由“帝国新兵换色”改为“寒地樵夫与边境猎户”。

#### 南境边民

- 第一套为赤膊苦役：不配置 `Body` 槽，使用农用双手锄斧、投石、南方头饰、披巾、绑带鞋和破旧绑臂。
- 第二套为荒漠贫民：使用阿塞莱民用头巾与民服、东方皮靴、镰刀、投石和破旧绑臂。
- 仅第一套赤膊，避免整支部队全部呈现竞技场奴隶或强盗观感，同时保留鲜明随机差异。
- 视觉定位为“被征召的荒漠苦役与边境佃农”，并提前预示 T2 的双手武器与投掷路线。

#### 西境边民

- 第一套使用软垫帽、麻布束腰衣、兜帽、绑带鞋与农用鹤嘴锄。
- 第二套使用皮帽、带水囊皮革民服、兜帽、绑带鞋与农用锤。
- 移除帝国新兵式步卒束腰衣和带护肩束腰衣。
- 视觉定位为“封建庄园农奴与边境工匠”；T2 才接受轻弩训练。

#### 东境边民

- 第一套使用游牧软帽、库赛特民用长衣、披巾与东方皮靴。
- 第二套使用粗皮帽、低阶草原服与东方皮靴。
- 两套继续固定猎弓、练习箭和农用长杆，不配马；不改变 `Ranged` 分组。
- 视觉定位为“徒步草原牧民与边境猎人”，T3 才完成骑射化。

### 3. T2 征召兵稳定地域标志

- 北境两套帝国软甲均增加 `fur_cloak_b` 低阶毛皮披肩，继续使用北方斧与圆盾。
- 南境两套分别固定 `wrapped_scarf` 南方披巾和 `desert_leather_shoulderpad` 沙漠皮护肩。
- 西境两套均固定 `hood` 兜帽，继续使用轻弩、单组弩矢和瓦兰迪亚短剑。
- 东境两套分别固定 `wrapped_scarf` 与 `steppe_leather_shoulders`，继续使用猎弓、练习箭、东方矛和东方皮靴。
- 四种 T2 继续保留帝国软甲 A/B，以表现刚进入帝国军需体系的共同身份。

### 4. 明确保留不变

- 八个 T1/T2 的兵种 ID、中文名称、等级、技能和战斗分组不变。
- 四条 `T1→T2→T3` 单线升级关系不变。
- T2 武器定位与东境 T3 才获得坐骑的规则不变。
- 旧共同 T2、原版帝国新兵追加入口、十二个 T3–T5、XSLT 金盔覆盖均不变。
- 本轮不实现聚落地域招募、边疆梯度概率、AI 升级权重或 DLL/Harmony 逻辑。

### 5. 静态校验

| 检查项 | 结果 |
|---|---|
| 模组 `spnpccharacters.xml` 解析与 `NPCCharacters.xsd` | 通过，0 个错误 |
| 自定义 NPC ID | 22 个，重复 0 |
| 八条 T1/T2 关键升级连接 | 8/8 正确 |
| 装备引用 | 128 个唯一物品 ID，缺失 0 |
| 南境 T1 赤膊装备方案 | 恰好 1 套未配置 `Body` |
| 四种 T2 地域肩部标志 | 8 套战斗装备全部包含 `Cape`，缺失 0 |
| XSLT 两种帝国 T5 远程兵金盔结果 | 通过，错误 0 |
| XSLT 转换后原版 NPC Schema | 通过，0 个错误 |
| 模块版本 | `v0.4.1` |

### 6. 部署

- 已将 7 个模块源码文件部署到游戏现有 `WXProvincialFreeCompany` 模块目录。
- 源码与安装目录逐文件 SHA-256 对比：缺失 0，差异 0。
- 安装目录 `SubModule.xml` 显示 `v0.4.1`。
- 未启动 Steam、Launcher、Modding Kit 或游戏；视觉效果继续由用户手动验收。

### 7. 手动验收项目

- [ ] 北境 T1 两套均显示毛皮兜帽、冻原布衣、高地裹腿与绑臂，没有退回帝国新兵轮廓。
- [ ] 南境 T1 赤膊方案正确显示身体、头巾、披巾和绑臂，没有默认内衣异常、身体缺失或明显穿模。
- [ ] 南境第二套完整显示南方民服，并与赤膊方案形成合理随机差异。
- [ ] 西境 T1 显示兜帽农奴/工匠轮廓，东境 T1 显示徒步游牧猎人轮廓。
- [ ] 四种 T2 的两套随机方案都能稳定看到各自地域肩饰。
- [ ] 毛皮披肩、南方披巾、兜帽、草原肩饰与帝国软甲及头巾没有明显穿模或领口遮挡。
- [ ] 本轮未引入技能、分组、武器切换、升级树、原版金盔或现有 T3–T5 回归问题。

### 8. Git 发布

- 功能分支：`feature/v0.4.1-regional-visual-identity`。
- 核心功能提交：`296ab8cc409640f07c2c265e3ae15cf8545de107`（`feat: strengthen regional low-tier visual identity in v0.4.1`）。
- 发布记录提交：`5f0c952c3b62fa5f8fe7947a4b25c0ef6a45d7c0`（`docs: record v0.4.1 validation and pull request`）。
- Pull Request：[PR #10：v0.4.1 重构地域低阶兵视觉身份](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/10)。
- PR 最终差异：7 个文件、164 行新增、36 行删除；GitHub 判定 `CLEAN / MERGEABLE`，仓库未配置 CI 检查。
- 2026-11-05 23:09（Asia/Shanghai）以 squash merge 合并到 `main`；合并提交：`8a7120b41460a2a3aced18b2719acd45d1fab79d`。
- 本地 `main`、`origin/main` 已同步到上述提交，工作区干净；安装目录与仓库源码 7 个文件的 SHA-256 对比为 0 项差异。

---

<a id="combat-record-026"></a>

## 战斗记录 022（原全局记录 026）：v0.4.2 独立地域树与低阶兵实测修正

**日期：** 2026-11-06  
**目标：** 根据自定义战斗实测，清除旧共同 T2 错误父节点，削弱东境 T1 的全能性，强化南境 T2 地域外观，并统一西境 T5 头盔  
**版本：** `v0.4.2-isolated-regional-trees`  
**开发分支：** `feat/v0.4.2-troop-tree-feedback`

### 1. 版本边界

本轮采用“先保证兵种数据与展示结构正确，后接战役征募”的过渡方案：

- 不再为旧共同 T2 保留存档兼容。
- 不实现 `VolunteerModel`、地域聚落映射、边疆概率、AI 征募或贵族兵保护代码。
- 四种地域 T1 暂时作为未接入志愿兵池的独立根节点存在，正常战役没有常规招募渠道。
- 自定义战斗继续按 `Culture.empire` 加载四条地域线的 T1–T5，是本版本的主要手动测试入口。

### 2. 兵种树清理

- 删除 `wx_provincial_levy` 旧“行省征召兵”完整兵种对象。
- 删除 `wx_provincial_levy_name` 中文文本。
- 删除模组对原版 `imperial_recruit` 追加 `wx_provincial_levy` 的合并节点。
- 原版帝国新兵树恢复为原生步兵与射手路线，不再显示错误的 MOD 共同父节点。
- 保留二十个地域兵种，形成四条相互独立、不会跨地域串线的五级路线：

```text
北境边民 T1 → 北境行省征召兵 T2 → 北境行省辅兵 T3 → 北境自由军盾斧兵 T4 → 北境自由军盾卫 T5
南境边民 T1 → 南境行省征召兵 T2 → 南境行省掷矛兵 T3 → 南境自由军突击兵 T4 → 南境自由军破阵卫 T5
西境边民 T1 → 西境行省征召兵 T2 → 西境行省弩手 T3 → 西境自由军重弩手 T4 → 西境自由军破甲弩卫 T5
东境边民 T1 → 东境行省征召兵 T2 → 东境行省骑射手 T3 → 东境自由军游骑射手 T4 → 东境自由军游骑卫 T5
```

### 3. 东境 T1 有限弹药猎户

实测确认旧方案同时拥有猎弓、27 支 `range_arrows` 和双手长柄镰刀，导致其在 T1 阶段既能持续远程输出，又能在接敌后依靠长柄挥砍继续收割。

v0.4.2 调整为：

- 新增 `Item.wx_crude_hunting_arrows`（粗制猎箭）。
- 箭袋复用原版 `range_arrows` 的箭矢、箭袋模型、飞行模型与声音，不新增模型或贴图。
- 容量固定为 9，箭矢附加穿刺伤害为 0，`is_merchandise="false"`，不会进入商店。
- 两套装备均将 27 支射程箭替换为 9 支粗制猎箭。
- 两套装备均将 `peasant_polearm_1_t1` 双手长柄镰刀替换为 `peasant_sickle_1_t1` 单手小镰刀。
- 弓术从 20 下调至 15；单手仍为 10，服装、无马状态和 `Ranged` 分组不变。

设计结果是保留东境猎户的开场远程身份，但将其限制为短时火力；弹药耗尽或被贴身后只能以低熟练度小镰刀自卫。

### 4. 南境 T2 地域视觉重构

旧方案两套都使用帝国头巾和帝国软甲，只有披巾、皮护肩与标枪提供南方信号，实测仍近似旧共同 T2。

新装备方案：

1. **帝国收编型：** `aserai_civil_e_hscarf` 阿塞莱包头巾、`empire_warrior_padded_armor_a` 帝国软甲、`wrapped_scarf` 南方披巾和 `eastern_leather_boots`。
2. **沙漠部族型：** `desert_cap` 沙漠帽、`desert_padded_cloth` 沙漠软垫布甲、`desert_leather_shoulderpad` 沙漠皮护肩和 `eastern_leather_boots`。

两套继续使用 `simple_sparth_axe_t2` 与单组 `western_javelin_1_t2`；等级、技能、步兵分组和升级关系不变。

### 5. 西境 T5 头盔统一

- 第一套移除 `heavy_nasalhelm_over_imperial_mail`。
- 两套战斗装备全部使用 `kettle_hat_over_mail_coif`（锅帽衬链甲围帽，原版头甲 42）。
- 弩、双组弩矢、剑/长柄偃月武器、身体护甲、技能和 `Ranged` 分组不变。

### 6. 静态校验

| 检查项 | 结果 |
|---|---|
| `spnpccharacters.xml` 解析与 `NPCCharacters.xsd` | 通过，0 个错误 |
| `wx_items.xml` 解析与 `Items.xsd` | 通过，0 个错误 |
| 自定义 NPC ID | 20 个，重复 0 |
| 四条地域升级链 | 4 条均为 T1→T2→T3→T4→T5，悬空目标 0 |
| 旧共同 T2 与帝国新兵覆盖 | 均已不存在 |
| 装备引用 | 缺失 0；NavalDLC 三件毛皮装备另经原始文件定位确认 |
| 中文文本引用 | 缺失 0 |
| 粗制猎箭 | 容量 9、附加伤害 0、物品注册路径 `wx_items` |
| 模块版本 | `v0.4.2` |

### 7. 部署

- 已将 8 个模块源码文件同步到游戏现有 `WXProvincialFreeCompany` 模块目录。
- 源码与安装目录逐文件 SHA-256 对比：缺失 0，差异 0。
- 安装目录 `SubModule.xml` 为 `v0.4.2`，安装目录粗制猎箭容量为 9。
- 未启动 Steam、Launcher、Modding Kit 或游戏；实际视觉、AI 和自定义战斗名单继续由用户手动验收。

### 8. 手动验收项目

- [ ] 自定义战斗帝国阵营仍可选择四条地域线全部 T1–T5，旧“行省征召兵”已经消失。
- [ ] 从四种地域 T1 查看时，各自只连接同地域 T2–T5，不出现跨地域分支。
- [ ] 东境 T1 两套均携带猎弓、9 支粗制猎箭和单手小镰刀；箭袋耗尽后能正常切换近战。
- [ ] 东境 T1 对其他 T1 仍有开场射击优势，但不再依靠 27 支箭与双手镰刀形成远近全能压制。
- [ ] 南境 T2 两套分别显示帝国收编型与沙漠部族型军容，头巾、披肩、软甲和鞋靴没有明显穿模。
- [ ] 西境 T5 两套随机装备均稳定显示锅帽衬链甲围帽。
- [ ] 北境线、其余西境/东境高阶兵、原版帝国远程金盔 XSLT 均无回归问题。

### 9. Git 发布与合并

- 功能分支：`feat/v0.4.2-troop-tree-feedback`。
- 功能提交：`2c52e50`（`feat: v0.4.2 清理兵种树并修正低阶兵反馈`），提交正文逐项记录兵种树、箭袋、三条兵种修正与版本边界。
- Pull Request：[PR #11：v0.4.2 清理旧公共兵种树并修正实测反馈](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/11)。
- PR 最终差异：8 个文件、100 行新增、96 行删除；GitHub 判定 `CLEAN / MERGEABLE`，仓库未配置 CI 检查。
- 2026-11-06 01:35（Asia/Shanghai）以 squash merge 合并到 `main`；合并提交：`9c35236b22f1479410ad8766bfea2802441cb7e3`。
- 本地 `main`、`origin/main` 已同步至 `9c35236`，Git 工作区干净；远端功能分支已随 PR 合并删除。

---

<a id="combat-record-028"></a>

## 战斗记录 023（原全局记录 028）：v0.4.3 粗制猎弓与北境海战专精

**日期：** 2026-11-06  
**目标：** 继续削弱东境 T1 的低阶远程压制力，并赋予北境整条兵种线清晰的海战接舷身份  
**版本：** `v0.4.3-crude-bow-naval-north`  
**开发分支：** `feat/v0.4.3-crude-bow-naval-north`

### 1. 实机反馈与设计决策

- v0.4.2 已将东境 T1 限制为 9 支箭、弓术 15 和单手小镰刀，但实测发现原版 `hunting_bow` 仍有 40 点穿刺伤害，单箭杀伤在 T1 对抗中依然过高。
- 北境 T3–T5 的盾、斧、重甲和毛皮军容已经稳定，但陆战定位仍接近普通持盾重步兵；本轮不继续抬高陆战数值，而是利用已有 NavalDLC 依赖将其确立为接舷步兵。
- 两项调整相互独立：粗制猎弓属于低阶战斗平衡；海军士兵特性属于海战专精，不作为北境陆战增益。

### 2. 新增粗制猎弓

新增 `Item.wx_crude_hunting_bow`（粗制猎弓），复用原版猎弓的模型、弓袋、持弓动作、声音和物理材质，不制作新模型或贴图，也不进入商店。

| 属性 | 原版猎弓 | 粗制猎弓 |
|---|---:|---:|
| 穿刺伤害 | 40 | 18 |
| 精度 | 85 | 72 |
| 弹速 | 64 | 52 |
| 射击速度 | 87 | 82 |
| 武器长度 | 110 | 110 |

东境 T1 两套随机装备的 `Item0` 均从 `Item.hunting_bow` 改为 `Item.wx_crude_hunting_bow`；9 支粗制猎箭、弓术 15、单手小镰刀、徒步状态和远程分组保持不变。其预期定位是接战前短时骚扰，而不是依靠低阶高穿刺弓持续击杀。

### 3. 北境全线海军士兵化

北境 T1–T5 全部新增：

```xml
<Traits>
  <Trait id="NavalSoldier" value="1" />
</Traits>
```

并参考 NavalDLC 诺德兵种的原生成长曲线配置航海熟练度：

| 兵阶 | 兵种 | Mariner |
|---|---|---:|
| T1 | 北境边民 | 50 |
| T2 | 北境行省征召兵 | 60 |
| T3 | 北境行省辅兵 | 70 |
| T4 | 北境自由军盾斧兵 | 80 |
| T5 | 北境自由军盾卫 | 90 |

本轮没有修改北境线的等级、武器、防具、跑动、单手、投掷、战斗分组或升级关系。其陆战强度原则上应与 v0.4.2 接近；差异主要体现在海战中免受普通陆军的移动、伤害和稳定性惩罚，以及海战自动结算和水下活动能力。

### 4. 静态校验与部署

| 检查项 | 结果 |
|---|---|
| `spnpccharacters.xml` 与 `NPCCharacters.xsd` | 通过，0 个错误 |
| `wx_items.xml` 与 `Items.xsd` | 通过，0 个错误 |
| XML 解析与 XSLT 编译 | 全部通过 |
| 自定义 NPC ID | 20 个，重复 0 |
| 自定义升级目标 | 悬空 0 |
| 东境 T1 武器 | 粗制猎弓引用 2，原版猎弓引用 0 |
| 北境水兵配置 | 五级均为 `NavalSoldier=1`，Mariner 为 50/60/70/80/90 |
| 非北境误配置 | 0 个兵种被误加 `NavalSoldier` |
| 本地化 | `wx_crude_hunting_bow_name=粗制猎弓`，引用唯一 |
| 模块版本 | `v0.4.3` |
| 安装一致性 | 仓库源码与安装目录 8 个文件 SHA-256 差异 0 |

已同步到游戏现有 `WXProvincialFreeCompany` 模块目录；没有启动 Steam、Launcher、Modding Kit 或游戏。

### 5. Git 发布与合并

- 功能提交：`fcd84ee`（`feat: v0.4.3 削弱东境猎弓并赋予北境海战专精`），正文记录物品数值、水兵成长、文档和校验结果。
- Pull Request：[PR #12：v0.4.3 粗制猎弓与北境海战专精](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/12)。
- PR 最终差异：6 个文件、82 行新增、8 行删除；GitHub 判定 `CLEAN / MERGEABLE`，仓库未配置 CI 检查。
- 2026-11-06 20:52（Asia/Shanghai）以 squash 方式合并到 `main`；合并提交：`907fb98cd95fd60bbd03da1ac7be0dddcdab8e66`。
- 本地 `main` 与 `origin/main` 已同步，工作区干净；远端功能分支已随 PR 合并删除。

### 6. 手动验收项目

- [ ] 在自定义战斗中确认东境 T1 两套均使用粗制猎弓、9 支粗制猎箭和单手小镰刀。
- [ ] 对比各文化 T1，确认东境 T1 仍能短时骚扰，但命中、弹道与单箭杀伤明显低于 v0.4.2。
- [ ] 在百科或兵种详情中确认北境 T1–T5 均具备海军士兵身份，航海熟练度依次为 50、60、70、80、90。
- [ ] 进行陆战回归测试，确认北境线武器选择、持盾接敌和同阶战斗力没有非预期变化。
- [ ] 进行海战对比，确认北境线的移动、伤害与稳定性明显优于同等装备的普通陆军，并能承担接舷作战。
- [ ] 观察重甲北境高阶兵落水后的表现，确认海军士兵加成没有使其成为无代价的重甲游泳兵。

---

<a id="combat-record-030"></a>

## 战斗记录 024（原全局记录 030）：v0.4.4 北境水兵门槛修正

**日期：** 2026-11-06  
**目标：** 将专业接舷能力从北方出身的固有属性，调整为进入自由军阶段后获得的职业训练成果  
**版本：** `v0.4.4-naval-tier-threshold`  
**开发分支：** `fix/v0.4.4-naval-tier-threshold`

### 1. 设计复核

v0.4.3 将北境 T1–T5 全部设为海军士兵，虽然形成了清晰的海战身份，但也使未经训练的边民、临时征召兵和普通行省辅兵天然免除主要海战惩罚。考虑到 `NavalSoldier` 同时影响实时海战和自动结算，这一范围偏强，也削弱了诺德与原版专业水兵的独特性。

v0.4.4 将门槛调整为：

```text
T1 北境边民              普通陆军，无 NavalSoldier，无 Mariner
T2 北境行省征召兵        普通陆军，无 NavalSoldier，无 Mariner
T3 北境行省辅兵          普通陆军，无 NavalSoldier，无 Mariner
T4 北境自由军盾斧兵      NavalSoldier=1，Mariner=80
T5 北境自由军盾卫        NavalSoldier=1，Mariner=90
```

### 2. 实现边界

- 从 T1、T2、T3 节点完整删除 `<Traits><Trait id="NavalSoldier" ... /></Traits>`。
- 同步删除三个低阶兵种的 `Mariner` 50、60、70，不保留“有航海技能但没有水兵身份”的半配置。
- T4、T5 的水兵特性与 80、90 航海熟练度保持不变。
- 不修改五级北境路线的等级、装备、跑动、单手、投掷、战斗分组或升级关系。
- 不修改东境粗制猎弓、南境、西境及原版帝国兵种。

### 3. 校验与部署

| 检查项 | 结果 |
|---|---|
| `spnpccharacters.xml` 与 `NPCCharacters.xsd` | 通过，0 个错误 |
| `wx_items.xml` 与 `Items.xsd` | 通过，0 个错误 |
| XML 解析与 XSLT 编译 | 全部通过 |
| 北境 T1/T2/T3 | `NavalSoldier=0`，`Mariner=0` |
| 北境 T4 | `NavalSoldier=1`，`Mariner=80` |
| 北境 T5 | `NavalSoldier=1`，`Mariner=90` |
| 其他兵种误配置 | 0 |
| 自定义 NPC | 20 个，重复 ID 0，悬空升级目标 0 |
| 模块版本 | `v0.4.4` |
| 安装一致性 | 仓库源码与安装目录 8 个文件 SHA-256 差异 0 |

已同步到游戏现有 `WXProvincialFreeCompany` 模块目录；没有启动 Steam、Launcher、Modding Kit 或游戏。

### 4. Git 发布与合并

- 功能提交：`09fe922`（`fix: v0.4.4 将北境水兵门槛调整至T4`）。
- Pull Request：[PR #13：v0.4.4 北境水兵门槛调整至T4](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/13)。
- PR 最终差异：4 个文件、15 行新增、18 行删除；GitHub 判定 `CLEAN / MERGEABLE`，仓库未配置 CI 检查。
- 2026-11-06 21:25（Asia/Shanghai）以 squash 方式合并到 `main`；合并提交：`34b0e3e448448af95eb78bdfeb0aa29fb38591cb`。
- 本地 `main` 与 `origin/main` 已同步，工作区干净；远端功能分支已随 PR 合并删除。

### 5. 手动验收项目

- [ ] 在百科中确认北境 T1–T3 不显示海军士兵身份，也不存在 50/60/70 航海熟练度。
- [ ] 确认北境 T4、T5 显示海军士兵身份，航海熟练度分别为 80、90。
- [ ] 陆战回归测试中，五级北境兵的装备、武器选择和同阶战力与 v0.4.3 一致。
- [ ] 海战中，T1–T3 应受到普通陆军惩罚，T4–T5 应表现为专业接舷重步兵。
- [ ] 自定义战斗和兵种树中仍能看到完整北境 T1→T5 升级路线。

---

<a id="combat-record-031"></a>

## 战斗记录 025（原全局记录 031）：v0.4.5 自定义战斗水兵标识修复

**日期：** 2026-11-07  
**目标：** 使北境 T4/T5 在战役与 War Sails 自定义战斗中使用一致的水兵身份  
**版本：** `v0.4.5-custom-battle-mariner`  
**开发分支：** `fix/v0.4.5-custom-battle-mariner`

### 1. 实测现象

v0.4.4 中，北境 T4“北境自由军盾斧兵”与 T5“北境自由军盾卫”在战役百科中正确拥有 `NavalSoldier=1` 和 80/90 点 `Mariner`，但在 War Sails 自定义战斗界面中仍显示为普通步兵。

这不是战役与自定义战斗使用了两份兵种配置。`SubModule.xml` 已将同一份 `spnpccharacters.xml` 同时加载到 `Campaign`、`CampaignStoryMode`、`CustomGame` 和 `EditorGame`。

### 2. 根因定位

对当前 War Sails `NavalDLC.CustomBattle.dll` 中 `NavalCustomBattleArmyCompositionItemVM.GetTroopTypeIconData` 的实际逻辑进行核对后确认，自定义战斗界面的“水兵/海军步兵”标识使用如下简化判定：

```text
兵种内部 ID 包含小写 "marine"
        或
文化内部 ID 包含 "nord"
        ↓
自定义战斗显示为 Infantry_Mariner
```

该界面逻辑不检查 `NavalSoldier` 特性，也不检查 `Mariner` 技能。官方非诺德水兵 ID，如 `empire_marine_t3/t4/t5`、`aserai_marine_t4/t5` 和 `vlandian_marine_t4/t5`，均遵循这一命名约定。北境高阶兵属于帝国文化，旧 ID 也不含 `marine`，因此界面把它们标为普通步兵。

### 3. 实现方案

| 阶级 | v0.4.4 内部 ID | v0.4.5 内部 ID | 结果 |
|---|---|---|---|
| T4 | `wx_northern_free_company_warrior` | `wx_northern_free_company_marine_warrior` | 符合自定义战斗水兵判定 |
| T5 | `wx_northern_free_company_veteran` | `wx_northern_free_company_marine_veteran` | 符合自定义战斗水兵判定 |

- 同步修改 T3→T4 与 T4→T5 的 `upgrade_target`，不留下悬空升级引用。
- 保留原有本地化 key，玩家可见的中英文名称不变。
- 不修改装备、等级、技能、`Infantry` 战斗分组、`NavalSoldier` 或 `Mariner`。`default_group="Infantry"` 仍是正确配置，官方水兵也沿用步兵阵型。
- T1–T3 的 ID 不含 `marine`，仍作为普通陆军显示和作战。
- 不引入 Harmony、DLL 或自定义战斗专用兵种副本，避免为一个界面分类问题增加持续维护成本。

### 4. 兼容性边界

本次修复必须改变两个兵种内部 ID。旧存档如果已经持有 v0.4.4 的北境 T4/T5，加载 v0.4.5 后可能出现丢失引用或兵种被移除。项目当前仍处于开发测试阶段，且已明确不保留旧测试存档兼容，因此不额外保留会在自定义战斗中造成重复项的旧 ID 别名。

### 5. 静态校验与部署

| 检查项 | 结果 |
|---|---|
| `spnpccharacters.xml` 与 `NPCCharacters.xsd` | 通过，0 个错误 |
| `wx_items.xml` 与 `Items.xsd` | 通过，0 个错误 |
| `spnpccharacters.xslt` | 编译通过 |
| 自定义 NPC | 20 个，重复 ID 0，悬空升级目标 0 |
| 模块版本 | `v0.4.5` |
| 水兵界面判定模拟 | T1–T3 为 `false`，T4–T5 为 `true` |
| 安装一致性 | 仓库源码与安装目录 8 个文件 SHA-256 差异 0 |

已将 v0.4.5 同步到现有 `WXProvincialFreeCompany` 游戏模块目录；本次没有启动 Steam、Launcher、Modding Kit 或游戏。

### 6. Git 发布与合并

- 开发分支：`fix/v0.4.5-custom-battle-mariner`。
- 功能提交：`580050d`（`fix: v0.4.5 修复自定义战斗水兵标识`）。
- Pull Request：[PR #14：v0.4.5 修复北境高阶兵的自定义战斗水兵标识](https://github.com/SuperShuangPowder/bannerLoard-mod/pull/14)。
- PR 差异：6 个文件、23 行新增、11 行删除；合并前状态为 `CLEAN / MERGEABLE`，仓库未配置 CI 检查。
- 2026-11-07 07:41（Asia/Shanghai）以 squash 方式合并到 `main`；合并提交：`4aca921be4dcdb0cae6909a270175ff13b2765d6`。
- 本地 `main` 与 `origin/main` 已同步，工作区干净；远端开发分支已删除。

### 7. 手动验收项目

- [ ] 在自定义战斗的帝国阵营中，确认北境 T4/T5 的类型文本/图标已从普通步兵变为水兵/海军步兵。
- [ ] 确认北境 T1–T3 仍显示为普通步兵。
- [ ] 确认自定义战斗中仍只有一组北境 T4/T5，没有旧 ID 导致的重复兵种。
- [ ] 在新战役的百科/兵种树中，确认北境 T3→T4→T5 升级关系完整。
- [ ] 在海战中回归验证 T4/T5 仍使用 `NavalSoldier=1` 与 `Mariner=80/90`，陆战装备和战力没有变化。

---

<a id="combat-next"></a>

## 里程碑与后续工作

> 阶段结论（2026-11-07）：v0.4.5 的北境水兵标识与四条地域 T1–T5 已随 v0.5.6 完成综合实机回归；战斗与兵种本体阶段正式封板。当前工作重心由“继续补齐兵种功能”转为新档长期游玩、战役生态观察与未来平衡迭代。

> 下方累计清单保留了各历史版本当时尚未单独勾选的验收项。它们用于记录真实开发过程，不再等同于当前版本的阻断项；后续综合版本已经覆盖或取代的项目不逐条倒填历史状态。

### 累计清单

- [x] 安装 Bannerlord Modding Kit，并确认 `Win64_Shipping_wEditor` 与 XML Editor 存在。
- [x] 冻结模块 ID 与目录名为 `WXProvincialFreeCompany`。
- [x] 创建并通过 `v0.1.0-skeleton` 空模块加载测试。
- [x] 创建 T2 `wx_provincial_levy` 单兵种原型并完成静态校验。
- [x] 由用户手动完成 `v0.1.1` 自定义战斗与升级树测试。
- [x] 加入简体中文本地化。
- [x] 设计并实现 T3 `wx_provincial_auxiliary`。
- [x] 设计并实现 T4 `wx_free_company_warrior`。
- [x] 设计并实现 T5 `wx_free_company_veteran`。
- [x] 完成 `v0.2.0` 全路线静态校验与安装一致性检查。
- [x] 创建本地 Git 仓库与公开 GitHub 仓库 `bannerLoard-mod`。
- [x] 将项目源代码整理到 `bannerlord-wx-provincial-free-company\src\WXProvincialFreeCompany`。
- [x] 建立 `v0.1.1` 与 `v0.2.0` 两个版本标签，并校验本地、远端主分支一致。
- [x] 记录行省自由军四地域分支、地域招募、战俘转化与海战扩展的长期设计构想。
- [x] 由用户手动完成 `v0.2.0` 战役、自定义战斗与基础功能测试；确认视觉和平衡仍需改进。
- [x] 实现 `v0.3.0` 四地域分支结构骨架，并完成静态校验与部署一致性检查。
- [x] 将 `feature/v0.3.0-regional-skeleton` 推送到 GitHub，并补充详细提交正文。
- [x] 创建 PR #1，以普通 merge commit 合并到 `main`，同步本地与远端主分支。
- [ ] 由用户手动完成 `v0.3.0` 四分支升级树、自定义战斗与新存档测试。
- [ ] 根据手动验收结果决定是否创建 `v0.3.0` Git 标签。
- [ ] 编写北、南、东、西四条路线的详细兵种设计表。
- [ ] 决定地域兵种 ID 重构与旧测试存档兼容策略。
- [x] 以北境盾斧步兵为第一条地域参考线，完成 `v0.3.1` 装备、技能与命名深化。
- [ ] 由用户手动完成 `v0.3.1` 北境线视觉、AI 武器选择与同阶战斗测试。
- [x] 完成 `v0.3.2` T2 行省征召兵的无盾双手冲击兵改造，并确认原版费用计算机制。
- [ ] 由用户手动完成 `v0.3.2` T2 三套装备、AI、战斗、生存率与费用测试。
- [x] 完成 `v0.3.3` 北境 T3–T5 毛皮肩甲分级与 NavalDLC 必需依赖声明。
- [x] 完成原版帝国禁卫射手、帝国军士弩手的禁卫射手盔覆盖。
- [x] 将 v0.3.3 功能分支推送至 GitHub，创建并以 squash 方式合并 PR #3。
- [x] 同步本地 `main` 与 `origin/main`，确认远端 7 个变更文件与本地最终内容一致。
- [ ] 由用户手动完成 `v0.3.3` 毛皮肩甲穿模、随机方案与帝国 T5 远程军容测试。
- [ ] 验证 T2 同时拥有四个 T3 升级目标时，玩家界面和 NPC 自动升级的表现。
- [x] 通过 PR #3 同步本开发日志的 v0.3.3 公开快照。
- [x] 根据用户实机反馈确认 v0.3.3 的原版远程兵装备节点被合并器追加，而不是替换。
- [x] 完成 v0.3.4 XSLT 精确头盔替换、Schema 校验与安装目录部署。
- [ ] 由用户手动完成 v0.3.4 两种帝国 T5 远程兵的稳定金盔与装备池验收。
- [x] 将 v0.3.4 修复分支推送至 GitHub，通过 PR #4 以 squash 方式合并到 `main`，并同步本地与远端主分支。
- [x] 完成 v0.3.5 南境 T3–T5 名称、双手武器、单组标枪、帝国/阿塞莱混合甲胄与技能重构。
- [ ] 由用户手动完成 v0.3.5 南境线外观、投掷切换、同阶战斗力与升级关系验收。
- [x] 将 v0.3.5 功能分支推送至 GitHub，通过 PR #5 以 squash 方式合并到 `main`，并同步本地与远端主分支。
- [x] 完成 v0.3.6 西境 T3–T5 名称、远程分组、轻弩/重弩/缠弦弩、双组弹药、长矛副武器与帝国/瓦兰迪亚混合军容。
- [ ] 由用户手动完成 v0.3.6 西境线远程阵型、弹药消耗、AI 切换、同阶平衡与视觉验收。
- [x] 将 v0.3.6 功能分支通过 PR #6 以 squash 方式合并到 `main`，并同步本地与远端主分支。
- [x] 完成 v0.3.7 东境 T3–T5 名称、骑射分组、单箭袋、弓马升级、长矛/帝国剑副武器与帝国/库赛特混合军容。
- [ ] 由用户手动完成 v0.3.7 东境线骑射 AI、箭尽切换、马匹升级需求、同阶平衡、下马作战与视觉验收。
- [x] 将 v0.3.7 功能分支通过 PR #7 以 squash 方式合并到 `main`，并同步本地与远端主分支。
- [x] 完成 v0.3.8 西境近战武器/技能修复、东境弓箭/技能/帝国马具修复，并保持北境、南境和升级入口不变。
- [ ] 由用户手动完成 v0.3.8 西境近战切换、T5 偃月武器、东境同级骑射对比与帝国马具验收。
- [x] 将 v0.3.8 修复分支通过 PR #8 以 squash 方式合并到 `main`，并同步本地与远端主分支。
- [x] 设计并实现北、南、西、东四套地域 T1/T2 的名称、装备、技能、数值基线与单线升级关系。
- [x] 完成 v0.4.0 地域 T1/T2 的 Schema、装备引用、升级链、本地化、XSLT 回归与部署哈希校验。
- [x] 将 v0.4.0 功能分支通过 PR #9 以 squash 方式合并到 `main`，并同步本地与远端主分支。
- [ ] 由用户手动完成 v0.4.0 八个新 T1/T2 的外观、AI 武器切换、同阶平衡与升级树验收。
- [x] 完成 v0.4.1 四种 T1 地域服饰重构与四种 T2 稳定地域肩饰配置。
- [x] 完成 v0.4.1 Schema、装备引用、升级链、赤膊方案、XSLT 回归与部署哈希校验。
- [x] 将 v0.4.1 功能分支通过 PR #10 以 squash 方式合并到 `main`，并同步本地与远端主分支。
- [ ] 由用户手动完成 v0.4.1 T1/T2 外观、赤膊显示、披肩穿模与视觉辨识度验收。
- [x] 完成 v0.4.2 旧共同 T2 与帝国新兵追加入口删除，四条地域 T1–T5 保持独立。
- [x] 新增 9 支粗制猎箭并完成东境 T1、南境 T2、西境 T5 实测修正。
- [x] 完成 v0.4.2 NPC/Items Schema、升级链、装备、本地化与部署哈希校验。
- [x] 将 v0.4.2 功能分支通过 PR #11 以 squash 方式合并到 `main`，并同步本地与远端主分支。
- [ ] 由用户手动完成 v0.4.2 自定义战斗名单、四条升级链、有限弹药、南境外观和西境锅帽验收。
- [x] 完成 v0.4.3 粗制猎弓数据物品、东境 T1 两套换装与中文本地化。
- [x] 完成北境 T1–T5 `NavalSoldier` 特性与 50/60/70/80/90 航海熟练度配置，保持陆战数据不变。
- [x] 完成 v0.4.3 NPC/Items Schema、升级链、物品引用、XSLT 与部署哈希校验。
- [x] 将 v0.4.3 功能分支通过 PR #12 以 squash 方式合并到 `main`，并同步本地与远端主分支。
- [ ] 由用户手动完成 v0.4.3 东境 T1 低伤弓、北境陆战回归与海战专精验收。
- [x] 完成 v0.4.4 北境 T1–T3 水兵特性与航海熟练度清理，仅保留 T4/T5 的 80/90 专业水兵配置。
- [x] 完成 v0.4.4 Schema、升级链、XSLT 与部署哈希校验。
- [x] 将 v0.4.4 修复分支通过 PR #13 以 squash 方式合并到 `main`，并同步本地与远端主分支。
- [ ] 由用户手动完成 v0.4.4 低阶普通陆军、高阶专业水兵及陆战无回归验收。
- [x] 定位 War Sails 自定义战斗的 `marine`/文化 ID 水兵标识逻辑，确认与战役 `NavalSoldier` 判定不同。
- [x] 完成 v0.4.5 北境 T4/T5 内部 ID 与升级引用修复，保持其他兵种数据不变。
- [x] 将 v0.4.5 修复分支通过 PR #14 以 squash 方式合并到 `main`，并同步本地与远端主分支。
- [ ] 由用户手动完成 v0.4.5 自定义战斗水兵标识、无重复项与新档升级树验收。
- [ ] 在 1.4.8 的 `SandBoxCore` 与 `NavalDLC` 数据中重新生成帝国平民兵种的最终合并树。
- [ ] 重新核对 T2、T3、T4、T5 技能模板和装备 ID。
- [ ] 研究并验证 1.4.8 的 Schema/XML 合并机制。

---

## 战斗与兵种设计阶段收尾（2026-11-07）

### 已完成的阶段成果

- 建立北、南、西、东四条独立 T1–T5 地域兵种树，并完成中英文名称、装备、技能、阵型和升级关系。
- 北境形成盾斧重步兵与 T4/T5 专业水兵路线；南境形成标枪＋双手武器突击路线；西境形成无盾重弩与近战自卫路线；东境形成帝国防护体系下的骑射路线。
- 完成低阶地域辨识、东境粗制猎弓/猎箭、西境锅帽、北境毛皮肩甲、帝国远程兵金色禁卫射手盔等视觉修正。
- 完成自定义战斗、战役百科、升级树与装备池的多轮实机检查；旧共同 T2 和错误公共入口已经清理。
- 北境 T4/T5 在战役与自定义战斗中的水兵身份一致，T1–T3 保持普通陆军。
- XML、XSLT、本地化、装备引用、Schema、部署哈希与Git/PR历史均形成可追溯工程链。

### 阶段判断

兵种系统已经从早期“帝国正规军弱化复刻”发展为具有地域身份和战术分工的完整体系。后续即使继续修改单兵装备或数值，也属于平衡和美术深化，不再是第一阶段缺失功能。

### 非阻断的未来候选项

- 根据长期战役伤亡、升级率和AI使用结果微调四条路线的强度与费用表现。
- 继续观察南境高爆发、西境近战自卫、东境对库赛特骑射手和北境陆海两用表现。
- 在不破坏帝国军容的前提下增加更多装备方案和视觉差异。
- 正式公开发布前，再做一次创意工坊下载版的自定义战斗与升级树回归。

### 封板决定

截至 `v0.5.6`，战斗与兵种设计第一阶段正式完成。后续发现的问题按新版本独立记录，不再继续扩张本阶段历史清单。

---

## 后续记录模板

```markdown
## 战斗记录 XXX：标题

**日期：** YYYY-MM-DD  
**目标：** 这一轮要解决什么问题  
**环境：** 游戏、工具、模块与加载顺序  

### 操作

1. ...
2. ...

### 结果

- ...

### 问题与证据

- 现象：
- 日志：
- 复现步骤：

### 决策

- 保留：
- 修改：
- 延后：

### 下一步

- [ ] ...
```

