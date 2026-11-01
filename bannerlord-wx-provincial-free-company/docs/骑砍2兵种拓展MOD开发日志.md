# 骑砍2兵种拓展 MOD 开发日志

> 项目代号：`WXProvincialFreeCompany`（暂定）  
> 项目名称：帝国行省自由军（暂定）  
> 文档用途：持续记录环境、需求、实现、测试、故障与版本决策  
> 建档日期：2026-10-31  
> 当前阶段：阶段 1——T2 单兵种原型与加载验证

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

## 记录 001：开发环境准备与校验

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

## 记录 002：兵种主题调整与共享升级树决策

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

## 记录 003：最小模块加载测试计划

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

### 9. 失败时的排查顺序

1. 模块目录是否多嵌套了一层。
2. 文件名是否精确为 `SubModule.xml`。
3. 模块ID、依赖ID和版本格式是否正确。
4. XML标签、引号和编码是否有效。
5. `XmlName` 的 `path` 是否与 `ModuleData\spnpccharacters.xml` 文件名一致。
6. `IncludedGameTypes` 是否包含正在测试的模式。
7. Launcher是否缓存了旧清单；完全退出后重新打开。
8. 日志中出现的第一个异常是什么；优先处理首个错误，不追逐后续连锁异常。

---

## 记录 004：通过Steam安装Modding Kit与搜索不到工具的排查

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

## 记录 005：Modding Kit 安装复检与目录差异

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

## 记录 006：T2“行省征召兵”单兵种原型

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

## 记录 007：简体中文与完整 T2–T5 兵种线

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

## 下一步

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
- [ ] 由用户手动完成 `v0.2.0` 中文、自定义战斗、升级链与存档测试。
- [ ] 在 1.4.8 的 `SandBoxCore` 与 `NavalDLC` 数据中重新生成帝国平民兵种的最终合并树。
- [ ] 重新核对 T2、T3、T4、T5 技能模板和装备 ID。
- [ ] 研究并验证 1.4.8 的 Schema/XML 合并机制。

---

## 后续记录模板

```markdown
## 记录 XXX：标题

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

