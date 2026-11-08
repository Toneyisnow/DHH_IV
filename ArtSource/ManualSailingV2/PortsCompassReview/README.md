# 手动航海：指南针与沿岸城市预览

接入正式 `SailingScene` 的东南亚 V2 运行时。

- 右下角指南针：复用 `Assets/Screenshot_Reconstruction/generated/06_compass_sheet.png` 第一格；连续旋转，北为 0°、东为 90°。读取与主船模型相同的世界方向，镜头绕转不改变航向。面板挡住航行点击和镜头滚轮。
- 城市：马六甲、马尼拉、巴达维亚，分别复用 `Assets/AutoSailing_Map_Pack/04_cities_small_v2/` 中的 `chinese_walled_port.png`、`chinese_palace.png`、`chinese_pagoda_port.png`。这是现有素材的港口概念预览，不是三座城市的历史建筑复原。
- 使用透明建筑裁切、真实世界尺寸、地形深度遮挡、昼夜色调和现有探索迷雾。底部锚点采样东南亚地形高度，位于陆侧低矮海岸。建筑采用面向镜头的 2.5D 图像，尚无多方向城市图或完整立体城镇。
- 城市名称只在完整探索区域内悬停显示；本次不增加入港/贸易交互。

游戏资源复制到 `MaritimeLegend/Assets/Resources/Sprites/ManualSailing/Ports/` 与 `Sprites/UI/ManualSailing/Compass/`，原素材保留。

验证入口：Development Player 参数 `-portsCompassSmoke`。创建独立临时游戏状态，不读取或修改玩家存档；覆盖四向航向、镜头独立性、点击遮挡、三城悬停、未知区域名称隐藏，并输出三张游戏截图。

2026-11-07 验证：Unity 6000.3.10f1 Windows 构建通过（`build-verified.log`）；16 项运行时断言通过，结果 `PORTS_COMPASS_PASS`（`runtime.log`）。三张实机截图为 `port-0.png`（马六甲）、`port-1.png`（马尼拉）、`port-2.png`（巴达维亚）。已检查透明边缘与海岸落点；建筑竖直放置，避免随相机俯角穿入山坡。山体遮住建筑中心时不显示名称。

可运行版本：`MaritimeLegend/Builds/PortsCompassPreview/MaritimeLegend.exe`。主工程无法直接批处理启动，本次使用已有 `.BuildWorkspace` 隔离目录编译，最终源码和资源均已保留在主工程。
