# Windows 稳定版架构深度资料

## 1. 目标与边界
- 工程路径：`/Users/cai/cai-code/jinchanchan/JinChanChanTool/SourceCode/JinChanChanTool`
- 技术栈：`.NET 8 + WinForms + Win32 P/Invoke + ONNX/OCR`
- 角色定位：长期可用的功能完整版（自动拿牌、自动刷新、阵容管理、向导、推荐数据、GPU 安装辅助）

## 2. 代码规模与复杂度
- `*.cs` 文件数：133
- 总代码行数：38615
- 主要超大文件：
  - `Forms/NecessaryForm/SettingForm.Designer.cs`（3679 行）
  - `Forms/NecessaryForm/MainForm.cs`（2600 行）
  - `Forms/NecessaryForm/SettingForm.cs`（1807 行）
  - `Forms/NecessaryForm/SetupWizardForm.cs`（1800 行）
  - `Services/CardService.cs`（1280 行）

## 3. 模块划分（按目录）
- `Forms/`：主窗体、设置窗体、向导窗体、覆盖层窗体、阵容编辑器。
- `Services/`：核心循环、配置服务、阵容/装备推荐、坐标配置、网络与下载、GPU 环境管理。
- `DataClass/`：`ManualSettings` / `AutomaticSettings` 与静态数据类型。
- `Tools/`：键鼠注入、阵容码工具、辅助工具。
- `Resources/`：模型、英雄数据、帮助文档。

## 4. 关键调用链
1. `Program.cs` 启动 WinForms，构建各类服务。
2. `MainForm` 负责 UI 事件、状态展示与热键联动。
3. `CardService` 负责主循环：截图 -> OCR -> 匹配 -> 执行动作 -> 状态回写。
4. `SettingForm`、`SetupWizardForm` 负责配置输入、校验、向导流转。
5. 推荐阵容与装备数据由 `Services/LineupCrawling`、`Services/RecommendedEquipment` 维护。

## 5. 架构优势
- 功能完整，实际流程覆盖广。
- 旧用户配置与热键语义稳定。
- 在 Windows 环境下能力成熟，问题场景积累多。

## 6. 架构问题
- UI 与业务耦合高：窗体直接触发和读取核心服务内部状态。
- `CardService` 职责过重：包含循环、匹配、刷新、执行、日志、异常处理。
- 异常处理分散：`catch` 多，定位路径长，可观测性不一致。
- 平台依赖深：`user32/gdi32` 与 WinForms 绑定，跨端迁移成本高。

## 7. 可复用资产清单（迁移价值高）
- 匹配策略与纠错经验（目标卡判定、刷新策略）。
- 配置字段与热键语义（用户习惯资产）。
- 阵容/装备推荐结构和历史数据。
- 运行时指标字段（`CaptureMs/OcrMs/MatchMs/ActionMs/LoopTotalMs`）。

## 8. 结论
Windows 稳定版的业务资产价值高，但架构可维护性已到瓶颈。迁移策略应是“抽取业务规则 + 保留配置语义 + 重建分层边界”，而不是继续在 WinForms 主体上叠补丁。
