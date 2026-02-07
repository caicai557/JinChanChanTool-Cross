# Baseline Diff Checklist

## 目的

冻结旧工程当前改动，作为新双端项目吸收基线。

## 当前仓库改动（需对账）

- `SourceCode/JinChanChanTool.sln`
- `SourceCode/JinChanChanTool/DataClass/ManualSettings.cs`
- `SourceCode/JinChanChanTool/Services/CardService.cs`
- `SourceCode/JinChanChanTool/Services/QueuedOCRService.cs`
- `SourceCode/JinChanChanTool/Services/RuntimeLoop/*`
- `SourceCode/JinChanChanTool/Tools/DebouncedSaver.cs`
- `SourceCode/JinChanChanTool.Tests/*`
- `SourceCode/perf-baseline.md`

## 吸收策略

1. 先吸收纯逻辑：匹配、刷新策略、性能指标。
2. 再吸收配置兼容字段：`UseNewLoopEngine`、`EnablePerfMetrics`、`CpuOcrConsumerCount`、`OcrWarmupEnabled`。
3. 平台相关改动只进 Platform 层，不进入 Core。

## 对账输出

每次吸收后记录：

- 来源文件
- 目标项目/目标类
- 行为兼容说明
- 回滚开关或回滚路径
