# 用 GitHub Desktop 把本仓库同步到你的 GitHub

## 一、你要用的本地文件夹

**只用这一个文件夹作为“仓库根目录”：**

```
…\Chill with You Lo-Fi Story\AIChat-1.8.3\AIChat-1.8.3
```

也就是说：GitHub 上的仓库 = 这个文件夹里的全部内容（含子文件夹），结构一致。

---

## 二、第一次：在 GitHub 上建一个空仓库

1. 打开浏览器，登录 **github.com**。
2. 右上角 **+** → **New repository**。
3. 填写：
   - **Repository name**：例如 `AIChat-Mod` 或 `chill-with-you-ai-mod`（任取，英文即可）。
   - **Description**：可选，例如「Chill with You 游戏 AI 对话 Mod」。
   - **Public**。
   - **不要**勾选 “Add a README file”、“Add .gitignore”、“Choose a license”（保持空仓库）。
4. 点 **Create repository**。
5. 记下仓库地址，形如：`https://github.com/你的用户名/仓库名.git`。

---

## 三、在 GitHub Desktop 里：两种做法选一种

### 做法 A：这个文件夹里还没有 .git（第一次当仓库用）

1. 打开 **GitHub Desktop**。
2. 菜单 **File** → **Add local repository**。
3. 点击 **Choose...**，选中文件夹：  
   `…\Chill with You Lo-Fi Story\AIChat-1.8.3\AIChat-1.8.3`
4. 若提示 “This directory does not appear to be a Git repository”：
   - 点 **create a repository**（或 **Create Repository**）。
   - **Local path** 保持就是这个 `AIChat-1.8.3` 文件夹。
   - **Git Ignore** 选 **None**（项目里已有 `.gitignore`）。
   - 点 **Create Repository**。
5. 然后跳到下面 **「四、第一次提交并推送到 GitHub」**。

### 做法 B：这个文件夹里已经有 .git（例如从别人那里拷来的）

1. 打开 **GitHub Desktop**。
2. **File** → **Add local repository**。
3. **Choose...** 选中的同样是：  
   `…\Chill with You Lo-Fi Story\AIChat-1.8.3\AIChat-1.8.3`
4. 若能正常识别为仓库，左侧会显示很多 “Changes”。
5. 把远程仓库改成**你自己的**：
   - 菜单 **Repository** → **Repository settings**（或 **Repository** → **Open in Command Prompt** 旁的设置）。
   - 在 **Primary remote repository** 里：
     - 若已有 **origin**：把 **URL** 改成你的仓库地址（见上面“二、5”）。
     - 若没有 **origin**：**Add** 一个，name 填 `origin`，URL 填你的仓库地址。
   - 保存/关闭。
6. 然后做 **「四、第一次提交并推送」**。

---

## 四、第一次提交并推送到 GitHub

1. 在 GitHub Desktop 左侧 **Changes** 里，你会看到当前文件夹里所有未被 `.gitignore` 忽略的文件。
2. 左上角 **Summary** 必填，例如：`Initial commit: AIChat Mod 源码与结构`。**Description** 可留空。
3. 点击左下角蓝色 **Commit to main**（或 **Commit to master**，依你默认分支名）。
4. 顶部菜单 **Repository** → **Push**（或 **Push origin**），把本地的 `main` 推上去。
5. 若提示 “Publish repository”：
   - 选 **Publish Repository**。
   - 名字和本地一致即可，**Keep this code private** 按需勾选。
   - 完成后，到 **github.com** 打开你的仓库，就能看到和本地一样的文件和文件夹结构。

---

## 五、之后：本地有修改时如何同步到网站

1. **保存你在本地的修改**（改代码、增删文件等）。
2. 打开 **GitHub Desktop**，左侧 **Changes** 会自动列出变更。
3. 勾选要提交的文件（默认全选即可）。
4. 左下角 **Summary** 写一句本次修改说明，例如：`修复点击角色无反应`、`更新 README`。
5. 点 **Commit to main**。
6. 再点顶部 **Push origin**（或 **Repository** → **Push**）。

这样，你本地 `AIChat-1.8.3` 里的修改就会同步到 GitHub 网站上；在网页里刷新仓库即可看到最新文件和提交记录。

---

## 六、简要对照表

| 目标 | 操作 |
|------|------|
| 第一次把当前文件夹变成“一个 GitHub 仓库” | 二（建空仓库）+ 三（A 或 B）+ 四（提交并推送） |
| 之后每次改完代码要同步到网站 | 五：保存 → 打开 GitHub Desktop → Commit → Push |

---

## 七、注意事项

- **只选一个文件夹**：仓库根 = `AIChat-1.8.3`，不要选上一级（会包含游戏本体等无关内容）。
- **.gitignore 已存在**：`bin/`、`obj/`、部分 BepInEx/Unity 的 DLL 等已被忽略，不会传上去，这是正常的。
- 若 **Push** 失败，提示没有权限：在 GitHub Desktop 里 **File** → **Options** → **Accounts** 确认已登录你的 GitHub 账号。

按上面步骤做完，你就会在 GitHub 上看到一个和本地 `AIChat-1.8.3` 文件系统一致的 repository，并且之后用 GitHub Desktop 的 Commit + Push 就能把本地修改同步到网站上。
