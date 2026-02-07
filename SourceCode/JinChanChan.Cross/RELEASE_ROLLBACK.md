# Release & Rollback

## 发布策略（仅新项目）

- 发布包 1：Windows x64（Avalonia 新项目）
- 发布包 2：macOS arm64（Avalonia 新项目）
- 旧 WinForms 项目保留为回滚包，不并线。

## 回滚触发条件

- 核心链路回归（自动拿牌/自动刷新/热键）
- mac 权限流程不可达
- OCR provider 触发不可恢复异常

## 回滚步骤

1. 停止分发新版本安装包。
2. 切换下载入口到旧 WinForms 包。
3. 保留新项目日志和迁移报告用于复盘。

## 诊断优先级

1. 先查 Core 层是否出现平台 API 泄漏。
2. 再查 OCR provider 回退链路是否失效。
3. 最后查平台权限与窗口句柄定位日志。
