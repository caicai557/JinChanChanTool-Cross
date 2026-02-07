# JinChanChanTool

这个仓库现在包含两套程序：

1. `SourceCode/JinChanChanTool`：Windows 稳定版（WinForms，功能最完整）  
2. `SourceCode/JinChanChan.Cross`：Windows + macOS arm64 双端重构版（Avalonia，当前是可用的重构版）

如果你只想直接用，优先用 Windows 稳定版。  
如果你要做双端开发或继续重构，用双端重构版。

---

## 项目实际功能

### 1. Windows 稳定版（`SourceCode/JinChanChanTool`）

已实现并可长期使用的功能：

- 商店 OCR 识别 + 自动拿牌
- 自动刷新商店（可与自动拿牌联动）
- 快捷键控制（开关、长按 D 牌、主窗口显示隐藏等）
- 阵容管理（导入导出、切换）
- 目标英雄高亮
- 推荐装备展示
- 多窗口/多显示器坐标配置
- CPU/GPU 推理模式切换

### 2. 双端重构版（`SourceCode/JinChanChan.Cross`）

当前代码里已经落地的功能：

- 启动时迁移旧配置：`ManualSettings.json`、`AutomaticSettings.json`
- 跨平台核心循环：截图 -> OCR -> 匹配 -> 执行操作
- Windows 与 macOS 的平台适配：
  - 截图
  - 鼠标键盘输入注入
  - 全局热键
  - 窗口查找
- macOS 权限检查与引导：屏幕录制、辅助功能
- OCR 统一 ONNX 引擎（支持 Provider 选择与 CPU 回退）
- 最小可运行循环：热键控制自动拿牌、自动刷新、长按 D 牌
- 基础测试工程：Core / Platform / E2E

当前还没做成完整版的部分（现状说明）：

- 双端版 UI 仍是最小可用界面，不是旧版那套完整面板
- 需要你提供模型文件和配置数据，才会得到稳定识别结果

---

## 如何使用

### A. Windows 稳定版（推荐普通用户）

1. 打开发布页：<https://github.com/XJYdemons/JinChanChanTool/releases>
2. 下载 Windows 压缩包并解压
3. 运行主程序
4. 按向导完成坐标、拿牌方式、刷新方式、OCR 设备设置
5. 选择目标英雄，开启自动拿牌/自动刷新

相关文档：

- `Documents/第1章 概述.md`
- `Documents/第2章 开始使用.md`
- `Documents/第3章 界面介绍.md`

### B. 双端重构版（开发者/测试者）

### 1）环境准备

- Windows 或 macOS（arm64）
- .NET 8 SDK

### 2）构建

在仓库根目录执行：

```bash
cd SourceCode
dotnet build JinChanChan.Cross.sln
```

### 3）运行桌面端

Windows：

```bash
dotnet run --project JinChanChan.Cross/JinChanChan.Desktop/JinChanChan.Desktop.csproj -f net8.0-windows10.0.17763.0
```

macOS：

```bash
dotnet run --project JinChanChan.Cross/JinChanChan.Desktop/JinChanChan.Desktop.csproj -f net8.0-macos14.0
```

### 4）必需文件

双端版按运行目录读取 `Resources`，至少要有：

- `Resources/Models/PP-OCRv5-mobile-rec.onnx`
- `Resources/Models/ppocr_keys_v1.txt`（建议提供）
- `Resources/ManualSettings.json`、`Resources/AutomaticSettings.json`（用于迁移旧配置）

### 5）首次可用配置

迁移后会生成：

- `Resources/Cross/AppSettings.json`
- `Resources/Cross/MigrationReport.json`

请确认 `AppSettings.json` 里的关键字段：

- `PreferredTargets`：目标英雄列表（为空就不会买牌）
- `Coordinates.CardNameRects`、`Coordinates.CardClickRects`、`Coordinates.RefreshButtonRect`
- `Hotkeys`：快捷键
- `UseKeyboardPurchase` / `UseKeyboardRefresh`

### 6）双端版默认热键语义

- `Hotkeys.ToggleAutoPick`：自动拿牌开关
- `Hotkeys.ToggleRefresh`：自动刷新开关
- `Hotkeys.HoldRoll`：按下持续 D 牌，松开停止
- `Hotkeys.ToggleMainWindow`：显示/隐藏主窗口

---

## 常见问题

### 1）为什么双端版启动后不自动买牌？

通常是三个原因：

- `PreferredTargets` 为空
- 坐标未迁移成功或坐标无效
- 模型文件不存在或路径不对

先看 `Resources/Cross/MigrationReport.json`，再看 `AppSettings.json`。

### 2）macOS 为什么没有反应？

先检查系统权限：

- 屏幕录制
- 辅助功能

两个权限都要放开。

### 3）如何反馈问题？

- 提交 Issue：<https://github.com/XJYdemons/JinChanChanTool/issues>
- 附带日志和配置文件（脱敏后）

---

## 代码结构（重构版）

- `JinChanChan.Core`：核心逻辑与接口
- `JinChanChan.Ocr`：统一 OCR 引擎
- `JinChanChan.Platform.Windows`：Windows 适配
- `JinChanChan.Platform.Mac`：macOS 适配
- `JinChanChan.Desktop`：桌面端入口
- `*.Tests`：测试工程

---

## 许可证

MIT，见 `LICENSE`。
