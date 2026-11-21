# 世界手动航海素材与生成

全球海岸轮廓：[Natural Earth 1:50m Land](https://www.naturalearthdata.com/downloads/50m-physical-vectors/50m-land/)。Natural Earth的栅格和矢量数据为公有领域：[使用条款](https://www.naturalearthdata.com/about/terms-of-use/)。下载原件保存在`Source/ne_50m_land.zip`，来源与SHA-256记录于`Review/generation.json`。

`build_world.py`读取保留的shapefile zip，生成东西周期距离场、带外圈采样的5°分块和现有19港口的粗略安全泊位。运行资源发布到`MaritimeLegend/Assets/Resources/Data/WorldMap/`。陆地高度与配色是程序化示意；不含准确地形高程。

`Review/world-overview.png`是制作审查总图，游戏不加载此全局高分辨率源数据。`runtime-*.png`是Unity编辑器离屏渲染，`player-*.png`是实际Player中渲染；日志与结果文本位于同目录。

`prepare_preview.py`生成隔离验证工程`UnityPreview/`（已忽略版本控制），编译同一套世界运行时源码。该目录的Build中提供本地可玩试航程序，不使用玩家存档。正式入口、实现范围、测试结果和待完成内容见`MaritimeLegend/Documentation/世界手动航海实现与验收.md`。
