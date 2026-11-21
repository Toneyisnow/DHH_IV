# 六地域城镇元素素材库

2026-11-08，依据用户提供的港口风格画面制作。六类：欧洲、北欧、地中海、非洲、亚洲、南美。

完整风格与运动规范见 [城镇六地域素材与慢动画规范](../../MaritimeLegend/Documentation/城镇六地域素材与慢动画规范.md)。

- `Reference.png`：用户参考，仅作风格参考。
- `Originals/<Style>/Elements.png`：内置 image_gen 的六套原始输出，保留不改写。
- `Workflow/prompts.json`：每套素材的完整生成提示词。
- `Workflow/Build-Library.ps1`：隔离 Unity 工程内生成、导出及渲染检查流程。
- `Review/`：Unity 渲染、动画预览及验证日志。

验收完成：6 套图集的透明轮廓与单元边界检查通过；36 个独立元素分别渲染两个时间点验证运动，飞鸟的振翅测试排除了轨迹位移；Shader 无编译错误。详见 `Review/motion-checks.csv` 和 `Review/PASS.txt`。

`Review/SixStyles.mp4` 为六种风格的 8 秒动态总览，每种风格另有独立 MP4。视频来自 Unity 每秒 12 帧的确定性渲染。`Review/SixStyles.png` 为总览首帧；保留每种风格第 00 和 95 帧作为首尾检查图。完整中间帧可用构建脚本重新生成，然后运行 `Workflow/Encode-Review.ps1` 编码。

每套六个元素：Road、House、Tree、Market、Eave、Bird。游戏版本位于 `MaritimeLegend/Assets/Resources/` 下的 Sprites、Prefabs、Models、Materials/Cities 分类目录。House 的风动挂饰为独立子层；石路仅浮尘运动；动画为网格权重变形、粒子与飞行轨迹组合，不是逐帧手绘重绘。

重建命令（仓库根目录）：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ArtSource/RegionalCityElements/Workflow/Build-Library.ps1
```

不覆盖旧 CityDaylight 素材，不自动替换已有关卡布局，不新增存档数据解释。
