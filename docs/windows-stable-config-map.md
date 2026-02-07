# Windows 稳定版配置映射资料

## 1. 旧配置文件
- `ManualSettings.json`
- `AutomaticSettings.json`

## 2. 新配置文件（双端）
- `Resources/Cross/AppSettings.json`
- `Resources/Cross/MigrationReport.json`

## 3. 映射原则
- 不破坏旧用户行为语义。
- 新字段只追加可选字段，保留默认值。
- 无法映射字段进入 `Legacy`，并写入迁移报告。

## 4. 关键字段映射

| 旧字段 | 新字段 | 说明 |
|---|---|---|
| `HotKey1` | `Hotkeys.ToggleAutoPick` | 自动拿牌开关热键 |
| `HotKey2` | `Hotkeys.ToggleRefresh` | 自动刷新开关热键 |
| `IsUseDynamicCoordinates` | `Coordinates.UseDynamicCoordinates` | 坐标模式 |
| `HeroNameScreenshotRectangle_*` | `Coordinates.CardNameRects` | 店铺名称截图区域 |
| `HeroClickRectangle_*` | `Coordinates.CardClickRects` | 购买点击区域 |
| `RefreshStoreButtonRectangle` | `Coordinates.RefreshButtonRect` | 刷新按钮区域 |
| `CpuOcrConsumerCount` | `CpuOcrConsumerCount` | OCR 并发参数 |
| `TargetProcessName` | `TargetProcessName` | 自动窗口绑定目标 |
| `TargetProcessId` | `TargetProcessId` | 进程 id |
| `RefreshStoreKey` | `RefreshKey` | 键盘刷新按键 |
| `HeroPurchaseKey1..5` | `PurchaseKeys` | 键盘买牌按键 |
| `UseNewLoopEngine` | `UseNewLoopEngine` | 主循环开关 |
| `EnableLineupAdvisor` | `EnableLineupAdvisor` | 阵容建议开关 |
| `EnableBenchSellHint` | `EnableBenchSellHint` | 卖牌建议开关 |
| `EnableCarouselHint` | `EnableCarouselHint` | 选秀建议开关 |
| `EnableAugmentHint` | `EnableAugmentHint` | 符文建议开关 |
| `AdvisorTickMs` | `AdvisorTickMs` | 建议刷新节拍 |
| `LineupDataSource` | `LineupDataSource` | 阵容数据源 |
| `OverlayOpacity` | `OverlayOpacity` | 覆盖层透明度 |
| `RecommendationStabilityWindow` | `RecommendationStabilityWindow` | 推荐稳定窗口 |

## 5. 默认值策略
- `EnableLineupAdvisor = true`
- `EnableBenchSellHint = true`
- `EnableCarouselHint = true`
- `EnableAugmentHint = true`
- `AdvisorTickMs = 200`
- `LineupDataSource = "local"`
- `OverlayOpacity = 0.85`
- `RecommendationStabilityWindow = 3`

## 6. 高风险字段
- 坐标相关（分辨率和窗口缩放差异会放大问题）。
- 热键相关（系统权限、平台按键语义差异）。
- OCR provider 相关（设备能力与驱动差异）。

## 7. 迁移验收要点
- 旧配置直接可启动。
- 热键语义与旧版一致。
- 关键流程（买牌/刷新/停止/重启恢复）一致。
