# Baseline Diff Checklist

## 目的

冻结旧工程当前改动，作为新双端项目吸收基线。

## 当前仓库改动（需对账）

- 旧 Windows 稳定版源码已从仓库主线移除。
- 删除前基线快照标签：`windows-stable-final`。
- 临时本地备份路径（用于紧急恢复）：`/tmp/jinchanchan-windows-stable-final`。
- 对账参考文档：`docs/windows-stable-architecture.md`、`docs/windows-vs-cross-gap-matrix.md`、`docs/cross-fill-priority.md`。

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
