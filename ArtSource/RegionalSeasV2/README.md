# 七海域原版风格重制

第二阶段现已接入主游戏，运行时使用这些区域高度场生成的统一分区世界，见 [流式世界实现与验证](../StreamedSeasV2/README.md)。下文保留第一阶段的独立地图制作流程和审查资源；独立区域目录仍不直接作为可拼接的全球坐标资源。

本阶段先制作地图美术，再做统一世界坐标及动态拼接。上一版 `ArtSource/WorldMap` 的独立陆地、水面渲染方案已被用户否定，不能作为本次画面基准。

## 地图和风格来源

北海、地中海、非洲、印度洋、东南亚、东亚、新大陆均使用仓库既有 `Assets/AutoSailing_Map_Pack/01_map/` 插画制作地形。七图使用同一份东南亚 `prepare_terrain.generate` 和 `SoutheastAsiaV2Study.BuildTerrain`，包括暖黄色山体、林地、海岸、近远两档网格、树石实例和海底深度图。

渲染直接实例化现有东南亚 Runtime，保留共享 Land 材质、树石模型、三种天空、太阳、船图、水面及所有 ManualSeaV2 Shader。每区仅替换陆地网格、实例位置、水深纹理；水材质仅更换 `_DepthMap` 和相同尺寸的 `_MapBounds`。水波、反射、泡沫、云影、船舶水面接触及相机算法均沿用原实现。

## 构建与审查

1. 根目录运行 `python ArtSource/RegionalSeasV2/prepare_regions.py`。自动核对东南亚 terrain 与原版逐字节一致。
2. 运行 `python ArtSource/RegionalSeasV2/prepare_preview.py`。复制原版 Prefab 的 GUID 依赖闭包到隔离 Unity 项目，不操作主工程恢复数据。审查项目保留主项目 Linear 色彩空间、质量和图形配置。
3. Unity 打开 `UnityPreview`，批处理执行 `MaritimeLegend.Editor.RegionalSeaV2Builder.BuildReview`。该方法生成七张地图资源与审查 Player。
4. 运行 Player：`RegionalSeaReview.exe -regionalReview <绝对 Review 目录> -logFile <绝对日志路径>`。调用生产 `ManualSeaV2Runtime` 输出七区晴天、夕阳、夜晚。审查不读取玩家存档，不把东南亚港口复制到其他海域。
5. 检查实景图和日志后运行 `python ArtSource/RegionalSeasV2/publish.py`。该步骤检查原版资源哈希，发布地图、GUID 和审查场景。

发布后运行 `python ArtSource/RegionalSeasV2/verify_assets.py`。当前核验：东南亚 360 个近远景网格与原资源逐字节相同，七区审查船位均在安全水域。实景总览为 `Review/seven-regions.png`。直接运行隔离项目中的 `Build/RegionalSeaReview.exe`（不带批处理参数）可切换七区及三种天空，右键环绕、滚轮缩放，退出程序不会保存游戏。

游戏资源在 `MaritimeLegend/Assets/Resources/{Models/Environment,Environment,Materials,Textures}/RegionalSeasV2/`。主游戏保留现有手动航海控制和东南亚场景；审查场景 `RegionalSeaV2Review` 是地图美术巡检，不是另一套游戏控制。

## 后续拼接的必要条件

现有七张插画是独立区域构图，具有不同尺度与重叠范围，不能把矩形图片边缘直接相连。资源目录明确标记 `georeferenced=false`、`streamingReady=false`，不冒充已经无缝的世界地图。

动态世界阶段先将海岸和海域美术配准到统一矩形经纬度空间，东南亚作为样式基准；同一全球高度场生成相邻块的共享边与法线，并从同一水深场取样。随后异步请求镜头覆盖区和预取环，保留迟滞卸载环；在主线程创建 Unity 对象并限制每帧提交量。维持原相机、点击航行、帆控、船队、水面、云影和天空，不能重新实现一套表现层。

东西循环要分别处理规范化存档经度、连续相机位置和边界副本；南北 ±80° 限制的是船队移动，不能靠切掉水面形成视觉断层。分区加载验收必须包含共享边、跨日期线、空气墙滑行、加载失败重试、快速转镜头、内存有界及原版画面对比。这里只记录下一阶段约束，不把旧世界试验的测试结果算作本阶段通过。
