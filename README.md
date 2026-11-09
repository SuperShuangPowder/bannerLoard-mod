# Bannerlord MOD 开发仓库

本仓库保存《骑马与砍杀2：霸主》MOD“帝国行省自由军”的正式源代码、构建脚本、测试和可追溯开发历史。游戏安装目录只用于本机部署与人工验收，不作为源码仓库。

## 当前项目

| 项目 | 当前稳定版本 | 阶段状态 |
|---|---|---|
| [`bannerlord-wx-provincial-free-company`](./bannerlord-wx-provincial-free-company/) | `v0.5.6-campaign-option-texts` | 第一阶段已完成；兵种、战役征募和关键实机验收均已通过 |

当前基线面向 Bannerlord `v1.4.8` 与 War Sails / NavalDLC `v1.2.8`。MOD包含四条地域T1–T5兵种树，以及基于官方 `VolunteerModel` 扩展机制的帝国地域征募系统；不依赖Harmony，也不携带任何官方游戏DLL。

## 新开发者与 Agent 从这里开始

1. 阅读项目的 [交接说明](./bannerlord-wx-provincial-free-company/PROJECT_HANDOFF.md)，先掌握当前状态、架构边界和下一步原则。
2. 阅读项目 [README](./bannerlord-wx-provincial-free-company/README.md)，了解兵种体系与当前功能。
3. 按 [BUILDING.md](./bannerlord-wx-provincial-free-company/BUILDING.md) 构建、测试、部署和打包。
4. 需要追溯设计演进时，再阅读两册开发日志：
   - [第1册：战斗设计部分](./bannerlord-wx-provincial-free-company/docs/骑砍2兵种扩展MOD开发日志-1战斗设计部分.md)
   - [第2册：战役设计部分](./bannerlord-wx-provincial-free-company/docs/骑砍2兵种扩展MOD开发日志-2战役设计部分.md)

针对自动化开发 Agent 的强制边界和验证规则位于仓库根目录的 [AGENTS.md](./AGENTS.md)。

## 仓库结构

```text
bannerLoard-mod/
├─ AGENTS.md
├─ README.md
└─ bannerlord-wx-provincial-free-company/
   ├─ PROJECT_HANDOFF.md
   ├─ README.md
   ├─ BUILDING.md
   ├─ code/       # C#源码
   ├─ design/     # 地域征募唯一设计源
   ├─ scripts/    # 校验、生成、构建、部署、打包
   ├─ src/        # 静态模块源，不等于完整发布包
   ├─ tests/      # 纯逻辑冒烟测试
   └─ docs/       # 当前两册开发日志与补充教程
```

## 当前工作原则

- `main`保持可构建、可部署的稳定基线；新功能使用独立分支。
- 不直接修改Bannerlord或NavalDLC官方文件。
- 不向Git提交官方DLL、存档、崩溃日志、账号凭据或本机绝对路径。
- `design/imperial_settlements.tsv` 是地域征募配置的唯一人工维护源；生成的XML和地图不得脱离设计源单独修改。
- 自动测试只能证明规则与产物没有明显回归；游戏加载、UI、战斗体验和长期战役生态仍需人工验收。
- 第一阶段已封板。未来版本应由新档长期游玩的真实反馈驱动，而不是为了继续开发而增加功能。

## 许可

本仓库目前尚未添加开源许可证。公开可见不等于自动授予复制、修改或再发布权利；正式公开分发前仍需选择许可证并完成创意工坊私密上传测试。

