# Windows 端到端循环性能基线

## 基线说明
- 采样对象: `CardService` 主循环 (`HighLight` / `GetCard`)
- 指标: `CaptureMs`、`OcrMs`、`CorrectionMs`、`MatchMs`、`ActionMs`、`LoopTotalMs`
- 统计方式: 平均值、P95、P99

## CPU 基线（待填写）
- 样本数:
- CaptureMs: Avg / P95 / P99
- OcrMs: Avg / P95 / P99
- CorrectionMs: Avg / P95 / P99
- MatchMs: Avg / P95 / P99
- ActionMs: Avg / P95 / P99
- LoopTotalMs: Avg / P95 / P99

## GPU 基线（待填写）
- 样本数:
- CaptureMs: Avg / P95 / P99
- OcrMs: Avg / P95 / P99
- CorrectionMs: Avg / P95 / P99
- MatchMs: Avg / P95 / P99
- ActionMs: Avg / P95 / P99
- LoopTotalMs: Avg / P95 / P99

## 记录规范
1. 每次改动前后都记录同场景 CPU/GPU 数据。
2. 每次记录附上提交号、配置摘要（分辨率、DPI、CPU/GPU模式、消费者数）。
3. 若 P95 回退超过 5%，阻断发布并回归定位。
