# 城市可操作主角接入 · 2026-11-07

按用户要求，城市主角统一显示最新 Character06 大头卡通行走素材。`ResolvePlayerVisual` 返回 6，仅控制城市显示；`ResolveCharacter` 的旧存档身份映射、存档数据与进度保持原状。居民仍使用现有 04/05 素材。

## 资源

- 来源：`Character06/Review168/walk-sheet.png`，内置 image_gen 生成并经过局部修帧、切分及统一锚点；不是 FrameRonin 生成结果。
- 游戏资源：`MaritimeLegend/Assets/Resources/Sprites/Characters/Character06/PixelWalk/walk-sheet.png`。
- 两份 SHA256：`144852C8B51F23D1D546AB3BA9C2A405ADD438A6A7BAB5C4E70E46A11029497B`。
- 840×1344 图集，168×168 单格，5 列、8 行；4 行走帧 + 1 独立停止帧。
- 原图行序 N、NE、E、SE、S、SW、W、NW；城市方向序 S、SW、W、NW、N、NE、E、SE，按 `(direction+4)%8` 映射。
- Point、无压缩、无 mipmap、不缩放 NPOT，脚底 pivot=(84,10)，PPU=64，场景缩放沿用 1.6。
- 整套烘焙服装，没有独立换装层；不会套用其他角色的换色/头饰编号。

## 控制与渲染

沿用 CityDiorama 的 WASD/方向键、鼠标寻路、台阶与碰撞、摄影机跟随。动画组件只切换 Sprite，不重复移动角色。松键保持方向并切到第 5 帧。沿用 CitySprite 材质和第 20 层，参与建筑深度遮挡、灯光及阴影。

## 检查

全量运行时代码 C# 编译通过：0 错误，2 条已有字段未赋值警告。
Unity 隔离构建成功，实际运行返回 `CITY_ART_PLAYMODE_PASS`，进程退出码 0。已验证新角色 40 个姿势的图集坐标、Point 采样、脚底锚点、停止保留朝向、动画不重复移动；原有八方向位移、台阶、桥梁、城市切换和 UI 回归通过。日志为 `city-build.log`、`city-runtime.log`，截图在 `UnityReview/`。

已查看正常城市镜头的 `UnityReview/Lisbon-before-UI.png`：新角色已作为场景中央的可操作主角出现，身体完整、脚底贴地、原城镇界面保留。旧测试的极近景 `Character6-direction*.png` 构图会裁到头顶，不作为完整人物的画面验收依据。

可运行构建：`MaritimeLegend/Builds/CityPreview/MaritimeLegend.exe`。加 `-cityDaylightPreview` 可进入隔离城市试玩；正常启动仍进入游戏流程。

当前动画仍是制作试稿，部分侧面/斜向步态需继续美术修正。此次按用户要求接入游戏，不代表八角色量产已完成。
