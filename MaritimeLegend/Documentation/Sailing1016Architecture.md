# 1016 补充设计：数据、公式与航海架构实现

本次是可运行的架构增量，不是1016设计全部功能的完成声明。主设计与补充设计正文均不修改。所有新增平衡初值仍是 mock；未进行玩家平衡测试。

## 数据入口

仍使用 `Assets/Resources/Data/manifest.json` 加载分类 JSON，单文件不超过16 KiB；参数配有 JSON Schema 和运行时 constraints，引用由 `Catalog.Validate` 检查。

| 路径（相对 Assets/Resources/Data） | 内容 | 状态 |
| --- | --- | --- |
| `Balance/sailing.json` | 游戏日倍率、粮水/精力/经验公式参数、转向/帆角速度、奇遇冷却与补给价格 | mock，可调 |
| `Sailing/regions.json` | 七海域ID、文本Key、地图资源Key、事件强度、四类相对权重、商品池与数量范围 | mock，使用补充设计示例权重 |
| `Sailing/encounters.json` | 漂流瓶、木箱、美人鱼、残骸的精力费用、结果Key、前置标记、品质 | mock；瓶/美人鱼先给线索标记，不保证商品 |
| `Sailing/defaults.json` | 现有八港到七海域的映射、各船型船员初值、补给技能ID | mock；不增建港口或Unity Scene |
| `Characters/backgrounds.json` | 四背景的稳定ID、剧情Key、开局倍率、材料礼包 | 纯占位，尚未应用福利或提供建角选择 |
| `Presentation/localization-plan.json` | 五种语言、源语言、回退语言、翻译完成状态 | 全游戏翻译未完成 |

`Tools/add_sailing1016_data.py` 是首次创作这些初值和 schema 的工具，重新运行会重置上述配置；日常调优直接编辑 JSON 及相应约束，不要把生成脚本当成启动步骤。

## 公式、时钟与结算

- `Core/Data/Formulas/SailingFormulas.cs`：粮水、精力、经验、平滑转向、角度换算和随机间隔的纯函数。`GameService.Navigation.cs` 继续负责现有风向/船况/载重航速公式和边界限制。
- `Core/Time/GameCalendar.cs`：1700-01-01 起，12月×30日。`elapsedGameDays` 为完成日数；`secondsIntoDay` 为当前日已累计的现实前台秒数；`dayDurationSeconds` 保存倍率分母，调倍率时保留当天完成比例。
- `Core/Sailing/GameService.Sailing.cs`：逐日结算玩家手动舰队，保存 `lastSettledDay`、本日实际支出和舰队经验。跨多个日界逐日执行；读取存档不会补扣历史费用。
- 公式初值：8船员、零技能修正，日耗粮1.6、水2.4、精力6、经验5。粮水保留小数；经验向下取整。导航/视野/补给取主角现有12技能中的对应等级；后勤效率只是系数，不新增技能。伙伴汇总、天气风险和探索加成接口暂未接入，当前后两者传0。
- 普通航行以旗舰为当前玩家舰队成员，船员来自实际 `Ship.crew` 字段。以后多船编队需显式配置成员，再将成员船员求和；不要把正在自动航线中的船也算入玩家舰队。
- 普通航行日扣粮水精力；靠港日、海战日不扣，是明确的暂定策略。当前日按跨日时的模式结算，尚未实现分段按小时计费。每日实际位移达到阈值、资源充足才发一次导航/舰队经验，停在地图边界或原地不会积累位移。缺补给减速且停发正常经验，保留返港救援。
- 玩家旗舰不再同时执行旧的 `foodPerSecond` 扣粮。自动运输仍走原有 `AdvanceAutoFleet` 的秒制物流，未迁入新日结算，也不触发玩家海面图标；后续接入时需统一每舰队结算器，避免双扣。
- `GameWorkflow` 统一驱动游戏时钟；暂停、既有剧情对话和海上询问都会冻结世界/位移/拒绝冷却。现实生产照常使用既有独立流程，只有生产暂停设置影响生产。
- 世界市场小时推进改为按游戏日倍率换算（24游戏小时/日）；现有UTC工厂时长与离线生产算法不改。离线读取不推进游戏日历。

## 奇遇与存档

`Core/Events/GameService.SeaEncounters.cs` 保存实例ID、类型、海域、位置、seed、固定结果、生命周期、领取状态、过期时间与 `ignoreUntil`。奖励使用既有商品ID，例如原木 `p19`，品质5；不创建重复商品。

接触半径由配置控制；`Available → Prompting → Resolved`。拒绝或精力/空间不足后回到 Available，单实例冷却10秒**前台普通航行时间**。接受前在临时货舱验证并构造结果，再同步提交货舱、精力和完成状态；重复碰撞/重复接受不会重新领奖。UI在进入询问及处理选择后自动保存，结果不在读档时重抽。主动恢复较早的整个存档仍会恢复较早的整个世界；本次不实现跨存档防回滚账本。

自动刷新依赖 `ISeaPlacementPolicy.IsNavigable(regionId,x,y)`，从 `GameService.SeaPlacement` 注入。默认无实现，因此默认不自动刷图标，避免把美术图的颜色当成真实海岸碰撞。接入导航网格/水域遮罩后才启用。测试使用显式的全水域夹具，不能用于生产地图。刷新机会采用指数间隔，长帧合并到每次调用至多一次刷新尝试，受到图标上限和水域验证约束，仍待空间刷新与长期经济模拟调优。

存档主版本保持1，新增 `GameState.sailing` 内部版本1。旧档缺扩展时在 GameService 构造中一次初始化新日历、精力与饮水；旧货币、货舱、订单与旧世界时间保持。0精力/0饮水是合法状态，不会被读档补满。地图位置、航向和旧trim字段继续作为唯一权威，避免两套位置不一致。

`ProgressionPlaceholders.cs` 预留章节耗时、活跃/离线时长、互斥事件链字段；尚未采集指标，不伪造测量结果。`ReplayProfile` 单独定义图鉴/外观/成就契约，没有金币、商品或船只，尚未实现文件存储或新周目继承。

## 场景与后续接入

- 继续使用一个 `SailingScene`。`SailingInput.cs` 只读输入并调用核心；鼠标按住地图渐变调整航向，A/D或左右键只调帆，R复位，W/S保留速度控制。内部沿用东向0的弧度与trim 0…1，HUD转换为北向0顺时针角度与帆角−90…90。
- HUD增加日期、海域、粮、水、精力、船员、经验和帆角；奇遇用临时方块、帆角用线段表现，后续替换为prefab/动画即可。
- `SetSeaRegion` 是海域切换入口。目前按出发/到达港配置切换；中途跨海域边界、局部地图坐标变换、海图资源加载仍待实现。`mapAssetKey` 是资源占位，当前仍显示已有示意地图，尚未导入根目录美术包为可导航地图。
- 经典模式允许海战，休闲模式拒绝。普通航行与模式状态切换保留航向/帆角；现有海战使用独立战斗坐标和旧键位，完整战斗输入共用/战斗中途存档恢复仍待扩展。加载存档继续按旧约定回到普通航行或城市。
- `Core/Localization` 提供Key查找/格式化/源语言回退，现已接入 Unity Localization 原生 `GameText` 五语表。`Resources/Localization` 保留导入种子及领域测试文本。设置页可即时切换并全局保存语言，主菜单、设置页和现有海上文本已接入；Steam语言检测、完整字体覆盖、旧商品名与多数界面尚未完成，不能宣称全游戏五语已完成。编辑器入口及覆盖范围见 `Unity本地化.md`。
- 背景剧情、互斥事件后续反馈、天气/洋流矢量、键位重映射、多船编队、按日自动舰队、真实海岸碰撞、完整本地化是下一批扩展点。Steam价格建议不进入游戏经济配置。

## 校验

从Unity工程根目录执行：

```text
python Tools/validate_game_data.py
dotnet run --project Tools/HeadlessChecks/HeadlessChecks.csproj
```

新增 `SailingArchitectureChecks` 同时提供 Unity 菜单 `Maritime Legend → Run sailing architecture checks`。测试包括日期进位、多日/分段结算、读档不重扣、停止/缺补给经验门槛、控制分离、模式限制、几何保护、固定奖励、10秒单实例冷却、失败不扣资源、接受幂等、旧档扩展迁移、生产时钟隔离、五语事件Key和10000次权重抽样。结果见 `Sailing1016TestResults.txt`。

本次无界面检查通过；使用安装的Unity程序集编译全部运行时代码通过（0错误，已有mapPan未赋值警告）。Unity批处理启动在执行测试前退出，日志 `sailing1016-editor.log`，因此尚未完成Editor/Play Mode交互验收。


## Manual sailing revision — 2026-10-24

The primary ship is drawn at the authoritative sea position. Each of the two visual escort ships now has a persisted world position and pixel-space heading (`manualFollowers`); missing legacy escort state starts gathered at the primary without changing save progress. Simulation advances in steps no larger than 1/60 second. Each escort turns toward its predecessor at the configured turn rate, moves continuously, and resolves its own coast collisions. The following distance grows from zero at rest to 2.25 map pixels underway. Rendering never repositions ships to match the primary heading. Escorts use a separate navigation instance so their contact flags cannot overwrite the primary HUD status.

Continuous skill effects can use `SkillResolver.Linear(points, maxSkillPoint, minValue, maxValue)`. The current skill definition's `maxLevel` is the existing stored name for maxSkillPoint; no skill/save IDs are reinterpreted. Sailing base speed now uses this interpolation from `navigation.minSailSpeed` (0.0014) to `navigation.maxSailSpeed` (0.0028). Commanded sailing speed is exactly `navigation.windSpeed * sailOpening * baseSailSpeed`, where configured wind magnitude defaults to 1 and W/S controls opening continuously from 0 to 4. Existing wind direction and A/D sail-angle visualization remain separate from this scalar formula. Previous hull, cargo, ship-type, settings and shortage speed multipliers no longer enter this formula. Coastal contact can reduce actual displacement speed.

Coast clearance defaults to zero. Distance interpolation follows the same two triangles per grid cell used to clip rendered Indian Ocean land, so shallow water is no longer excluded by the former 20-pixel margin.

Validation: from the MaritimeLegend directory run `dotnet run --project Tools/ManualSailingChecks/ManualSailingChecks.csproj`; alternatively use Unity's `Maritime Legend/Run manual sailing checks`. The standalone harness compiles the real domain, coast and follower sources with test-only resource/serialization/vector adapters. It covers skill endpoints/midpoint, formula values, actual terrain/coast agreement, shallow movement, swept land collisions, bounded turns/displacements, stop/restart/bend behavior, save roundtrip and the existing sailing architecture suite. Also run `python Tools/validate_game_data.py`.

Both standalone checks passed on 2026-10-24. Unity batch startup exited with code 1 before loading the project; full Unity compilation and rendered play-mode verification were not completed in this session.

Follow-up tuning: maximum following gap reduced from 9 to 2.25 pixels (one quarter); both sailing speed endpoints reduced to one fifth. Escort minimum catch-up speed and full-gap speed threshold reduced from 12 to 2.4 pixels/second to preserve low-speed behavior.
