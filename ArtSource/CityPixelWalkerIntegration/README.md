# 城镇八方向像素角色集成（2026-11-04）

## 输入与显示

- Character04：`ArtSource/Character04PixelWalk/AcceptedV14/Runtime/`，与Resources中的两张图集逐字节一致；保留原来的侧面头部pivot修正。
- Character05：现有Resources中的FitV3图集与分层Prefab，保留头饰后绳、遮发身体和前后装备层。
- `inputs-sha256.json`：本次使用的45张图集及2个Prefab校验值。没有生成或重画角色。
- 城镇显示通过`CityPixelWalker`复用现有动画组件；以CitySprite材质参与建筑深度遮挡、灯光与投影。64×96格、每方向四帧行走/一帧停止，PPU=32，统一1.6倍场景缩放（2026-11-05按用户要求由0.8放大为原来的2倍），脚底pivot为8/96。分层按原排序作小幅深度偏移。
- 城镇移动与寻路不交给预览组件，避免WASD触发两份移动。
- 已保存04/05明确身份及获批的槽位1旧西亚女性预览可用；2026-11-05按用户补充要求，其余主角身份也使用Character04新像素图作为临时显示，不修改保存的身份。港口居民使用两套现有素材。完整映射边界见Documentation下的1104 TODO。

## 验证

- C#全量运行时代码编译：0错误；已有mapPan未赋值警告1项。
- 完整Windows构建的前置DomainChecks被旧商品数量断言阻塞：期望50项；本次未修改经济数据或该断言。
- 使用已有CityArtSetup.BuildPreview入口构建城镇验证版本：构建成功，实际运行返回CITY_ART_PLAYMODE_PASS。结果见Review/PASS.txt、runtime.log和build.log。
- 运行验证通过两角色全部80个姿势、图集坐标、点采样、停止保留方向、动画不自行移动、显式身份兼容、里斯本台阶、威尼斯桥梁、八城地标可达性和天气/UI操作。
- 截图仅证明接入画面；不代表Character05全部5376种换装组合已人工验收。

已查看实际渲染：Character04正面/背面、Character05侧面/背面、Town-layout-UI全场景，人物完整、脚底与道路对齐，原城镇UI框架保留。验证版：`MaritimeLegend/Builds/CityPreview/MaritimeLegend.exe`，加`-cityDaylightPreview`直接进入城市；加`-cityArtSmoke -batchmode -force-d3d11`重跑自动验证，工作目录使用MaritimeLegend。

## 2026-11-05 主角修复

旧问题：主角身份不在04/05或特批旧档映射内时仍创建CityDaylight/Walker旧图。修复后，主角统一通过ResolvePlayerVisual建立CityPixelWalker，未映射身份使用Character04基础外观；不改变ResolveCharacter的身份判断或写入存档。回归入口刻意使用baseId=2、空portraitV2、slot=6，检查实际主角对象没有旧Walker，以及进入八城后的移动与停止。

2026-11-05修复验收：构建成功，CITY_ART_PLAYMODE_PASS；旧身份主角使用新图且没有旧Walker对象，台阶/桥梁行走和独立停止帧通过。

## 2026-11-05 键盘地图方向修正

移除键盘输入的网格Snap+BFS寻路，改为地图X/Z连续移动；单键锁定一个轴，双键归一化斜移，朝向直接取地图方向，松键不再追赶剩余路径。以小步采样地面高度、建筑阻挡和斜向角点，保留台阶通行，禁止穿墙和踏水。鼠标点击保留原寻路。回归从非网格中心起步测试八方向精确位移、统一速度、朝向、松键停止及取消旧鼠标路径。
八方向精确位移、朝向、归一化速度、松键停止、鼠标路径取消和高台墙阻挡均通过；完整城镇运行回归返回CITY_ART_PLAYMODE_PASS。

## 2026-11-05 所有人物放大

主角与城镇居民共用CityPixelWalker.Create，统一缩放从0.8改为1.6。沿原脚底锚点等比放大，分层装备同步放大；移动速度、地图坐标、寻路与碰撞规则保持原值。
