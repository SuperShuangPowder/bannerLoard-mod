# Bannerlord WX Provincial Free Company

中文工作名：帝国行省自由军。

本项目为《骑马与砍杀 2：霸主》`v1.4.8` 制作一条使用原版资产的帝国平民兵种支线。

## 当前版本

`v0.2.0`：T2–T5 完整原型兵种线，并加入简体中文本地化。

```text
帝国新兵（原版 T1）
└─ 行省征召兵（T2）
   └─ 行省辅兵（T3）
      └─ 自由军团战士（T4）
         └─ 自由军团老兵（T5）
```

`v0.1.1` 标签保留了已经通过实机测试的 T2 原型。当前 `v0.2.0` 已完成静态校验，等待完整兵种线实机验证。

## 仓库结构

```text
bannerlord-wx-provincial-free-company/
├─ docs/                       # 开发日志与补充教程
├─ src/
│  └─ WXProvincialFreeCompany/   # 可部署的 Bannerlord 模块
└─ README.md
```

## 安装测试

将 `src/WXProvincialFreeCompany` 整个文件夹复制到 Bannerlord 的 `Modules` 目录，在启动器中启用模块后进行测试。请先备份存档；开发版本不承诺向前或向后兼容。

## 安全边界

- 不修改 Native、SandBoxCore、Sandbox、StoryMode、CustomBattle 或 NavalDLC 官方文件。
- 不收录游戏存档、崩溃日志、账号凭据或官方游戏资产。
- 当前版本不包含 DLL、Harmony 补丁或第三方运行时依赖。

