# 手动航海 V2：大地图、镜头与三种天空

## 单独测试

停止 Play，等待 Unity 编译完成。使用菜单 `Maritime Legend > Manual Sailing V2 > Play Test Scene` 重新打开并运行场景（已经打开的旧场景需要重新加载）。
场景：`MaritimeLegend/Assets/Scenes/ManualSailingV2Test.unity`。

- WASD：相对于镜头水平朝向平移。
- 按住鼠标右键左右拖动：绕当前观察中心旋转，只改变 Yaw。
- 滚轮：改变观察距离；R：恢复 35°、默认位置与距离。
- 1 / 2 / 3，或左上方按钮：晴天 / 夕阳 / 星夜。
- Main Camera 的 `Manual Sea V2 Camera` 组件：`Pitch` 默认 35，`Sky Height Fraction` 默认 0.15；运行时可修改，也可通过 `Pitch` C# property 设置俯角。
- 相机根据俯角和天空比例计算透视 FOV。海平线约在屏幕高度 85% 处（自下而上）；陆地可以遮挡局部天空。右键旋转不会改变这个比例。
- `V2 Environment` 的 `Manual Sea V2 Environment` 组件中选择 Mode，可在编辑器中预览光照。
- 水材质的 Wave Length / Wave Strength / Wave Speed 控制大浪的尺度、明暗与速度。

`Create Test Scene` 会重新生成并覆盖本测试场景及专用资产，手动修改请另存副本。

## 已实现

640×480 地图，面积是初版 160×120 的 16 倍，24 个岛屿，11,272 个树木/礁石实例。
普通山顶与下部岩土保持暖黄色；只有高海拔且深入内陆的位置才向深黄色渐变。北部 6 个测试岛屿在高处叠加不规则雪线和白雪。
海水改为蔚蓝色、青绿色浅滩，加入大尺度双向涌浪法线、日/月高光路径与夕阳暖色反射。
三种程序化 Skybox：晴朗蓝天；橙红色太阳与晚霞；星星、明月和月光倒影。
后续星空修订：星点总密度约为初版 3 倍，每颗星的 Shader 亮度系数为初版 4 倍。
东南亚区域版已另建 `SoutheastAsiaSailingV2.unity`，使用 `Play Southeast Asia` 菜单，详见 `SoutheastAsia/README.md`。
远处水面根据像素覆盖范围减弱细波纹，减少地平线附近的混叠。

## 性能措施

- 岛屿复用 3 种形状 × 普通/雪山颜色，各两级 LOD；不为每个岛重复存储地形。
- 近景地形网格步长 1.25，远景 2.5（初版 0.42）；省去深水区域的地形三角形。
- 树木/礁石只使用 3 组共享模型，各有简化远景模型；不创建一万多个 GameObject。
- 40 单位分区视锥剔除，GPU instancing 每批最多 1023 个实例；不支持实例化时有普通绘制回退。
- 海洋保持 2 个三角形，波浪和反射由 Shader 完成，没有额外反射相机。
- 水深纹理 1536×1152，约 6.75 MiB 原始浮点数据；每帧不重新生成地形或水深。

## 预览与验证

主要艺术参考始终保留于 `Reference/primary-visual-reference.png`。
最新预览：`Review/overview.png`、`Review/northern-snow.png`、`Review/sky-0.png` 至 `sky-2.png`。
旧 `coast-detail.png`、`overview-motion.png` 仅为初版历史预览，不代表当前地图。

Unity 6000.3.10f1 / Built-in / Direct3D 11 独立工程执行 Shader 检查、场景生成和 Windows Development Player 构建。
另对主工程所用的新 Input System 分支执行 C# 编译检查。

性能原始结果见 `Review/performance.txt`；实际运行截图为 `Review/runtime-*.png`。
测试强制 1600×900 离屏渲染，每帧 1 像素 GPU 回读以同步；包含同步开销，不应称为交互窗口 FPS 或独立 GPU 帧耗时。
隐藏窗口普通帧循环会跳过绘制，因此已弃用它的虚高帧率。

## 范围与资源

当前山树仍是程序化低面数样板，尚未替换成精细手绘 Tiles。相机可以水平旋转，因此后续手绘素材需要多方向图或立体承托。
海面反射是方向性天空/日月光照近似，不是场景物体的实时平面反射；海浪没有几何位移、物理浮力或船尾流。
尚未加入船、港口、海鸥和游戏会话。

游戏资源在 Resources 下按 Models/Environment、Textures、Materials 分类。
正式生成器：`MaritimeLegend/Assets/Editor/ManualSeaV2Study.cs`。
独立验证工程 `UnityPreview/` 和日志不提交；不修改主工程的渲染管线、构建场景列表或用户 ProjectSettings。
