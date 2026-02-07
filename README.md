# JinChanChan

JinChanChan 是一个面向《金铲铲之战》辅助流程的双端桌面项目，主线为 `Avalonia + Core + 平台适配`，支持 Windows 与 macOS arm64。

## 项目实际功能

### 1. 自动循环能力
- 商店截图 + OCR 识别
- 目标卡匹配
- 自动拿牌（鼠标或键盘）
- 自动刷新（鼠标或键盘）
- 热键控制自动拿牌、自动刷新、长按 D 牌、主窗口显隐

### 2. 智能建议能力
- 动态阵容推荐（按匹配度排序）
- 资源管理辅助（冗余棋子出售建议、选秀优先级）
- 装备与符文建议（主 C 三件套、进度、符文主备选）

### 3. 兼容与迁移能力
- 启动时自动迁移旧配置：`ManualSettings.json`、`AutomaticSettings.json`
- 生成新配置：`Resources/Cross/AppSettings.json`
- 生成迁移报告：`Resources/Cross/MigrationReport.json`

### 4. 跨平台适配能力
- Windows：截图、输入注入、热键、窗口绑定
- macOS arm64：截图、输入注入、热键、窗口绑定、权限检测与引导

## 代码结构

- `SourceCode/JinChanChan.Cross/JinChanChan.Core`：核心接口与业务逻辑
- `SourceCode/JinChanChan.Cross/JinChanChan.Ocr`：ONNX OCR 引擎
- `SourceCode/JinChanChan.Cross/JinChanChan.Platform.Windows`：Windows 适配
- `SourceCode/JinChanChan.Cross/JinChanChan.Platform.Mac`：macOS 适配
- `SourceCode/JinChanChan.Cross/JinChanChan.Desktop`：桌面程序入口
- `SourceCode/JinChanChan.Cross/*.Tests`：测试工程
- `docs/`：架构分析、差异矩阵、优化计划资料

## 如何使用

### A. 直接使用发布包（推荐）
1. 进入发布页下载对应系统包。
2. 解压后运行主程序。
3. 首次启动完成权限与窗口绑定。
4. 检查配置里的目标英雄和坐标。
5. 用热键开启自动拿牌和自动刷新。

### B. 本地开发运行

#### 1）环境准备
- .NET 8 SDK
- macOS 目标构建需要完整 Xcode（不是只装 CommandLineTools）

#### 2）构建
```bash
cd SourceCode
~/.dotnet/dotnet build JinChanChan.Cross.sln
```

#### 3）运行（Windows 目标）
```bash
~/.dotnet/dotnet run --project JinChanChan.Cross/JinChanChan.Desktop/JinChanChan.Desktop.csproj -f net8.0-windows10.0.17763.0
```

#### 4）运行（macOS 目标）
```bash
~/.dotnet/dotnet run --project JinChanChan.Cross/JinChanChan.Desktop/JinChanChan.Desktop.csproj -f net8.0-macos14.0
```

## 必需资源

运行目录下需要：
- `Resources/Models/PP-OCRv5-mobile-rec.onnx`
- `Resources/Models/ppocr_keys_v1.txt`
- `Resources/Data/lineup_templates.json`

可选（用于迁移旧配置）：
- `Resources/ManualSettings.json`
- `Resources/AutomaticSettings.json`

## 测试命令

```bash
~/.dotnet/dotnet test SourceCode/JinChanChan.Cross/JinChanChan.Core.Tests/JinChanChan.Core.Tests.csproj
~/.dotnet/dotnet test SourceCode/JinChanChan.Cross/JinChanChan.E2E.Tests/JinChanChan.E2E.Tests.csproj
```

## 常见问题

### 1）启动后不自动买牌
优先检查：
- `PreferredTargets` 是否为空
- 坐标是否有效
- 模型文件是否存在

### 2）macOS 没有动作
优先检查：
- 屏幕录制权限
- 辅助功能权限
- 是否安装完整 Xcode（构建时需要）

## 许可证

MIT，见 `LICENSE`。
