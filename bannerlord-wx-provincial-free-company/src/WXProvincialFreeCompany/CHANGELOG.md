# Changelog

## v0.3.0-regional-skeleton — 2026-11-02

- 保留共同 T2 `wx_provincial_levy`，从 T3 开始新增北、南、东、西四个升级目标。
- 新增四条独立的 T3–T5 地域路线，共十二个地域兵种对象。
- 为十二个地域兵种加入简体中文名称与英文后备名称。
- 四个 T3、四个 T4、四个 T5 分别复制 `v0.2.0` 同等级配置，以隔离升级树结构测试与后续兵种设计。
- 移除三个通用 T3–T5 对象的升级入口，改用具有明确地域含义的新 ID。
- 已知限制：四条路线目前外观、装备、技能和战斗力完全相同；视觉辨识度与平衡问题留待后续版本逐条修正。
- 存档说明：不保证兼容包含旧通用 T3–T5 兵种的 `v0.2.0` 测试存档。

## v0.2.0-full-line-prototype — 2026-11-01

- 新增 T3 `wx_provincial_auxiliary`（Provincial Auxiliary／行省辅兵）。
- 新增 T4 `wx_free_company_warrior`（Free Company Warrior／自由军团战士）。
- 新增 T5 `wx_free_company_veteran`（Free Company Veteran／自由军团老兵）。
- 将 T2 至 T5 串联为完整单线升级路线。
- 为四个新增兵种加入简体中文本地化。
- 全线使用 SandBoxCore 原版装备，不依赖 War Sails 专属资产。
- 高阶兵种强化长杆、投掷与跑动，降低单手与护甲预算，避免成为帝国军团步兵的完全上位替代。

## v0.1.1-t2-prototype — 2026-11-01

- 新增 T2 `wx_provincial_levy`（Provincial Levy／行省征召兵）原型。
- 使用帝国 T2 原版装备制作两套随机装备配置。
- 向 `imperial_recruit` 追加 `wx_provincial_levy` 升级目标。
- 保留原版 `imperial_infantryman` 与 `imperial_archer` 两条升级路线。
- T3 与中文本地化留待原型加载验证通过后加入。

## v0.1.0-skeleton — 2026-11-01

- 创建模块身份 `WXProvincialFreeCompany`。
- 注册空的 `NPCCharacters` 数据文件。
- 支持 Campaign、CampaignStoryMode、CustomGame 与 EditorGame 加载环境。
- 声明 Bannerlord `v1.4.8` 的 Native 与 SandBoxCore 为必需依赖。
- 尚未添加兵种、升级路线或代码逻辑。
