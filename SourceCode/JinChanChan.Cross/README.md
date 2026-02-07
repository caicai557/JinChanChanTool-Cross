# JinChanChan Cross

跨平台重构工程（Windows + macOS arm64），采用以下分层：

- `JinChanChan.Core`: 纯业务核心与接口，不依赖 WinForms 和平台 API。
- `JinChanChan.Ocr`: 统一 ONNX OCR 入口（含 provider 选择和回退机制）。
- `JinChanChan.Platform.Windows`: Windows 平台适配（截图/输入/窗口定位/权限）。
- `JinChanChan.Platform.Mac`: macOS 平台适配（权限/窗口定位/输入/截图）。
- `JinChanChan.Desktop`: Avalonia 桌面端入口。
- `*.Tests`: Core / Platform / E2E 测试。

## 目标

1. 新项目双端发布，旧 WinForms 项目保留作回滚包。
2. Core 层禁止出现 `System.Windows.Forms`、`user32.dll`、`gdi32.dll`。
3. 启动时自动执行旧配置迁移，输出迁移报告。

## 构建

- 解决方案：`SourceCode/JinChanChan.Cross.sln`
- Windows: `net8.0-windows10.0.17763.0`
- macOS: `net8.0-macos14.0`（arm64）

## 注意

- mac 端首版已接入权限检查、窗口自动绑定、截图像素提取与输入注入链路。
- mac 端首版已接入 Carbon 全局热键注册（按键按下/抬起回调）。
- OCR 已接入 ONNX 推理前处理 + CTC 解码，支持 provider 选择与 CPU 回退。
- 桌面端已接入最小可运行循环：热键控制自动拿牌、自动刷新、长按D牌。
