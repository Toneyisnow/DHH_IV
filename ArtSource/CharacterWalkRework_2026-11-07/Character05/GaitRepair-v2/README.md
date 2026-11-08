# 非裔男性步态修复 v2

用户指出：SW、NE、W重复半周期，多个斜向顺边。以认可的SE（右下）和E（正右）为基准，修复SW、NE、NW、W。

- SE与E全部像素不变；N、S不变。
- SW、W：以对应已认可步态镜像作为运动参考，使用内置imagegen修正腰包遮挡。镜像仅作运动参考，不能直接把不对称装备带到另一侧。
- NE：第一次整排行走修图仍重复1/3，弃用其接触帧；保留原第一帧，以单帧重绘制作反向接触帧，再单独修正近腿/远腿连接。中间帧取各自不同的过渡姿势。
- NW：使用NE修正后的运动参考，独立修正装备遮挡。
- 每帧头部上方70px完全沿用旧图，保留脸、发型和原有起伏。
- 15张行走帧实际改变；NE第1帧保留，右下/正右完全保留。
- 独立停止姿势保留。仅W停止帧腰包区域做局部遮挡修正，避免停走时错误露出近侧腰包；其余停止帧像素不变。

## 人工逐帧审阅

这不是仅靠图片像素不同或Unity计数器作结论。对contact-review.png逐帧追踪肩—肘—手及髋—膝—鞋：

|方向|第1帧近侧手臂|第3帧近侧手臂|近侧腿|
|---|---|---|---|
|SW|后摆|前摆|与手臂反向|
|W|后摆|前摆|与手臂反向|
|NE|前摆|后摆|与手臂反向|
|NW|前摆|后摆|与手臂反向|

第2/4帧为不同脚的通过/支撑姿势；NE/NW可见两次不同抬膝与鞋底朝向。以4fps慢速循环查看 repaired-walk.gif；contact-review.png用于并排比较1/3和2/4。

## 输出与重建

- Before/：修复前40帧，勿覆盖。
- FinalFrames/：规范化的重绘片段。
- ../Review168/、../Layers/、../Loop/、../walk-review-slow.gif：最终源资源和八方向预览，已同步。
- 游戏资源：MaritimeLegend/Assets/Resources/Sprites/Characters/Character05/CartoonWalk/walk-sheet.png。
- 原 Play-African-Male.cmd 试玩入口继续有效。

运行根目录 repair-gait05.cjs --nw → publish-gait05.cjs → finish-04-05.cjs 05。最后复制05的Review168/walk-sheet.png至游戏CartoonWalk目录，保留.meta。不要再单独运行旧pack-character05.cjs来发布，它只会重建初稿。

## 检查范围

checks.json检查修改范围；原头部像素、SE/E及未指定方向不变。三层复合仍与最终整图逐像素一致。
Unity完整城市运行检查输出CITY_ART_PLAYMODE_PASS，未发现Exception/FAILED。新增四方向×四帧的真实渲染截图位于UnityReview/，已目视复查NE反向接触帧与W反向接触帧。运行检查后仅追加W停止帧腰包小区域修正，最终构建包含该修改；四个修复方向的行走帧与已测版本完全相同。

本次针对Character05；没有宣称其他角色的步态也已修复。

后续最新局部修正：../.. /MotionPolish-v4（路径为 ../../MotionPolish-v4）。当前 Review168 与游戏资源已同步，旧版 FinalFrames 为历史记录。
