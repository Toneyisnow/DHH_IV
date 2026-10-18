# 航海世家（Maritime Legend）

Unity 项目位于 `MaritimeLegend/`。

## 项目文档

- [游戏设计文档（1015版）](MaritimeLegend/Documentation/航海世家_游戏设计文档_1015.md)：用户提供的详细设计文档，2026-10-15 已补充城市初始库存规则；后续设计查阅与更新使用此项目内副本。
- [补充设计（1016版·纯设计架构版）](MaritimeLegend/Documentation/航海世家_1016版补充设计_纯设计架构版.md)：2026-10-16 原样收录的独立增量补充，尚未合并；不覆盖或替代1015版主设计文档，后续由用户定期合并。
- [规则与公式](MaritimeLegend/Documentation/规则与公式.md)：当前实现公式、初始化规则与游戏数据位置。
- [1016航海架构实现](MaritimeLegend/Documentation/Sailing1016Architecture.md)：新增数据、mock参数、日历与奇遇存档、公式入口、已接入功能和后续扩展边界。
- [操作说明](MaritimeLegend/Documentation/操作说明.md)：当前游戏操作说明。

设计文档描述目标要求；实际实现与验收情况以项目代码和测试记录为准。

## 数据与验证入口

- [游戏数据清单](MaritimeLegend/Assets/Resources/Data/manifest.json)：按类别拆分的 JSON，单文件上限16 KiB。
- [数据加载器](MaritimeLegend/Assets/Core/Data/CatalogLoader.cs) / [公式代码](MaritimeLegend/Assets/Core/Data/Formulas/) / [用户数据路径](MaritimeLegend/Assets/Core/SaveSystem/UserDataPaths.cs)。
- 模拟夹具在 `MaritimeLegend/Assets/Tests/Data/`；存档和用户设置使用 Unity `Application.persistentDataPath`，不属于 Resources 游戏数据。
- 在 `MaritimeLegend/` 执行 `python Tools/validate_game_data.py` 和 `dotnet run --project Tools/HeadlessChecks/HeadlessChecks.csproj`。完整经营模拟追加 `-- --campaign`，当前有终章制作资金不足的已知失败，详见设计文档下一轮计划。
