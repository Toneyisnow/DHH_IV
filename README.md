# 航海世家（Maritime Legend）

Unity 项目位于 `MaritimeLegend/`。

## 项目文档

- [游戏主设计文档（最新版，2026-10-23整合）](MaritimeLegend/Documentation/航海世家_游戏设计文档_最新版.md)：唯一现行设计入口；已合并1015、1016、1023和最新品质/角色美术确认。
- [1015历史来源](MaritimeLegend/Documentation/航海世家_游戏设计文档_1015.md)、[1016历史补充](MaritimeLegend/Documentation/航海世家_1016版补充设计_纯设计架构版.md)、[1023历史补充](MaritimeLegend/Documentation/航海世家_1023补充设计文档.md)：保留追溯，不与主文档并列生效。
- [规则与公式](MaritimeLegend/Documentation/规则与公式.md)：当前实现公式、初始化规则与游戏数据位置。
- [1016航海架构实现](MaritimeLegend/Documentation/Sailing1016Architecture.md)：新增数据、mock参数、日历与奇遇存档、公式入口、已接入功能和后续扩展边界。
- [操作说明](MaritimeLegend/Documentation/操作说明.md)：当前游戏操作说明。

设计文档描述目标要求；实际实现与验收情况以项目代码和测试记录为准。

## 数据与验证入口

- [游戏数据清单](MaritimeLegend/Assets/Resources/Data/manifest.json)：按类别拆分的 JSON，单文件上限16 KiB。
- [数据加载器](MaritimeLegend/Assets/Core/Data/CatalogLoader.cs) / [公式代码](MaritimeLegend/Assets/Core/Data/Formulas/) / [用户数据路径](MaritimeLegend/Assets/Core/SaveSystem/UserDataPaths.cs)。
- 模拟夹具在 `MaritimeLegend/Assets/Tests/Data/`；存档和用户设置使用 Unity `Application.persistentDataPath`，不属于 Resources 游戏数据。
- 在 `MaritimeLegend/` 执行 `python Tools/validate_game_data.py` 和 `dotnet run --project Tools/HeadlessChecks/HeadlessChecks.csproj`。完整经营模拟追加 `-- --campaign`，历史记录有终章制作资金不足的失败，详见主设计文档历史实现记录；当前状态需重新验证。
