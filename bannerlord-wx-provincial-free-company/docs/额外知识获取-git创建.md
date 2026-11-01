# 额外知识获取

> 主题：在 Windows 上安全配置 GitHub CLI，并让 Codex 协助管理 GitHub 仓库  
> 建档日期：2026-11-01  
> 适用环境：GitHub.com、Windows、Git、GitHub CLI

---

## 1. 本文解决什么问题

在本地完成 GitHub 官方授权后，可以让 Git、GitHub CLI 与 Codex 在不共享密码、访问令牌或 SSH 私钥的前提下协同工作。

适合执行的任务包括：

- 创建 GitHub 仓库；
- 初始化并整理本地 Git 仓库；
- 提交源码与开发文档；
- 推送分支与版本标签；
- 查看远程仓库、Issue、Pull Request 和工作流状态；
- 在用户明确授权后创建 Pull Request 或发布版本。

不应把 GitHub 密码、Personal Access Token、SSH 私钥、验证码或恢复码发送到聊天中。

---

## 2. 安装前检查

在 PowerShell 或 Windows Terminal 中运行：

```powershell
git --version
gh --version
```

如果 Git 已安装、但 `gh` 提示无法识别，则需要安装 GitHub CLI。

---

## 3. 安装 GitHub CLI

使用 GitHub 官方推荐的 WinGet 安装方式：

```powershell
winget install --id GitHub.cli --source winget
```

安装完成后，关闭整个终端窗口并重新打开，然后运行：

```powershell
gh --version
```

Windows 安装程序会修改 PATH。已经在安装前启动的终端、IDE 或 Codex 进程可能继续使用旧 PATH；只新建一个终端标签页不一定足够。

如果新终端可以运行 `gh`，而旧程序仍然找不到它，可以重新启动旧程序，或者临时使用默认完整路径：

```text
C:\Program Files\GitHub CLI\gh.exe
```

---

## 4. 使用浏览器安全登录

推荐使用浏览器授权和 HTTPS Git 协议：

```powershell
gh auth login --hostname github.com --git-protocol https --web
```

一般流程如下：

1. GitHub CLI 显示一次性设备码；
2. 浏览器打开 GitHub 官方页面；
3. 用户亲自登录 GitHub；
4. 用户输入或确认设备码；
5. 用户检查权限并授权 GitHub CLI；
6. GitHub CLI 将授权信息保存到 Windows 凭据存储。

不要把设备码、验证码或令牌交给其他人。浏览器授权页面必须属于 `github.com`。

如果 GitHub CLI 提示只能把令牌保存为明文文件，应停止操作并先排查凭据存储问题，不要使用 `--insecure-storage` 规避警告。

---

## 5. 让 Git 使用 GitHub CLI 的登录状态

运行：

```powershell
gh auth setup-git
```

随后检查登录状态：

```powershell
gh auth status --hostname github.com
```

正常结果应包含：

```text
Logged in to github.com account ...
Active account: true
Git operations protocol: https
```

不要运行或分享下列命令的输出：

```powershell
gh auth token
gh auth status --show-token
```

---

## 6. 配置 Git 提交身份

GitHub 登录状态与 Git 提交身份是两件事。还需要设置提交记录中显示的姓名和邮箱：

```powershell
git config --global user.name "你的显示名称"
git config --global user.email "你的 GitHub 邮箱或 noreply 邮箱"
```

如果不希望公开真实邮箱，可前往 GitHub：

```text
Settings → Emails
```

复制 GitHub 提供的 `noreply` 邮箱，不要凭空猜测其格式。

检查配置：

```powershell
git config --global user.name
git config --global user.email
```

不要在准备公开的教程、仓库 README 或截图中记录私人邮箱。

---

## 7. 登录状态能否长期复用

通常可以。只要满足以下条件，未来在同一台电脑、同一 Windows 用户下运行的 GitHub CLI 就可以继续使用已经保存的授权：

- Windows 凭据存储中的 GitHub CLI 凭据仍然存在；
- GitHub 授权没有过期或被撤销；
- 当前使用的是正确的 GitHub 账号；
- 当前操作没有超出已授予的权限范围；
- 网络、组织 SSO 和仓库访问策略允许该操作。

这不代表 Codex 获得了无限期、无限范围的账号控制权：

- 每次会话仍受工具、网络与文件权限限制；
- Codex 只应执行用户当前请求范围内的操作；
- 创建仓库、推送代码、创建 Pull Request、发布版本或删除远程内容都属于外部状态变更，应在执行前明确说明目标；
- 不应因为本机已经登录，就擅自浏览或修改无关仓库；
- 删除仓库、分支、Release 等破坏性操作需要单独确认。

因此，更准确的理解是：**GitHub CLI 保存了可复用的登录凭据，而不是向某一次 Codex 会话永久移交账号。**

---

## 8. 多账号与状态检查

查看当前账号：

```powershell
gh auth status --hostname github.com
```

如果登录了多个账号，可查看并切换：

```powershell
gh auth switch --hostname github.com
```

进行仓库创建或推送前，应先确认活动账号正确，避免把私人项目推送到工作账号或其他组织账号。

---

## 9. 权限不足或授权异常

如果 GitHub CLI 已登录，但某项操作提示权限不足，可先检查：

```powershell
gh auth status --hostname github.com
```

需要修复或重新确认授权时，可运行：

```powershell
gh auth refresh --hostname github.com
```

不要为了省事直接创建高权限 Personal Access Token，也不要随意增加组织管理、仓库删除等与当前任务无关的权限。

组织仓库还可能要求单独完成 SSO 授权；个人账号登录成功不等于自动获得组织仓库权限。

---

## 10. 退出与撤销授权

从当前电脑移除 GitHub CLI 登录配置：

```powershell
gh auth logout --hostname github.com
```

本地退出不一定会撤销 GitHub 服务器端已经签发的全部授权。若要彻底撤销，可在 GitHub 打开：

```text
Settings → Applications → Authorized OAuth Apps → GitHub CLI → Revoke Access
```

撤销后，未来再次使用 GitHub CLI 时需要重新进行浏览器授权。

---

## 11. 推荐的 MOD 仓库工作流

不要把游戏安装目录当作唯一源代码位置。推荐结构：

```text
仓库根目录
├─ src
│  └─ WXProvincialFreeCompany
│     ├─ SubModule.xml
│     ├─ ModuleData
│     ├─ README.md
│     └─ CHANGELOG.md
├─ docs
│  └─ 骑砍2兵种拓展MOD开发日志.md
├─ scripts
│  └─ 校验与部署工具
├─ .gitignore
└─ README.md
```

工作方向：

```text
Git 仓库中的源代码
        ↓ 校验
        ↓ 部署
Bannerlord\Modules\WXProvincialFreeCompany
        ↓ 用户手动实机测试
        ↓ 记录结果
Git 提交与版本标签
```

游戏目录只是测试部署位置；Git 仓库才是可追溯、可回滚的正式源代码。

---

## 12. 创建仓库前应确认的事项

在创建远程仓库前，至少确认：

- 仓库名称；
- 所有者账号或组织；
- 公开或私有；
- 仓库描述；
- 默认分支名称；
- 只收录 MOD，还是包含整个独立游戏开发计划；
- 是否包含许可证；
- 哪些本地文件必须通过 `.gitignore` 排除。

开发早期推荐使用私有仓库。确认不包含存档、崩溃日志、个人路径、账号信息、第三方受限资产或其他敏感内容后，再考虑公开。

---

## 13. 本机本次校验记录

**校验日期：** 2026-11-01  

| 项目 | 状态 |
|---|---|
| Git | 已安装 |
| GitHub CLI | 已安装，版本 `2.103.3` |
| GitHub CLI 登录 | 已确认成功 |
| 凭据保存位置 | Windows keyring／系统凭据存储 |
| Git 协议 | HTTPS |
| 活动账号 | 已确认 |
| Git 提交姓名 | 已配置 |
| Git 提交邮箱 | 已配置，文档不记录具体地址 |

本次校验没有读取或输出未遮罩的 GitHub 令牌。

---

## 14. 官方资料

- GitHub CLI Windows 安装：<https://github.com/cli/cli/blob/trunk/docs/install_windows.md>
- GitHub CLI 登录：<https://cli.github.com/manual/gh_auth_login>
- 查看登录状态：<https://cli.github.com/manual/gh_auth_status>
- 配置 Git 凭据：<https://cli.github.com/manual/gh_auth_setup-git>
- 刷新权限：<https://cli.github.com/manual/gh_auth_refresh>
- 退出与撤销说明：<https://cli.github.com/manual/gh_auth_logout>

