# 东南亚航行地图 V2

范围按用户确认：中南半岛、马来群岛、菲律宾。地形轮廓来自仓库已有 `Assets/AutoSailing_Map_Pack/01_map/southeast_asia.png`；本次附件 `east_asia.png` 仅作绘画风格参考，不混用东亚的地理布局。

## 运行

- Unity：`Maritime Legend > Manual Sailing V2 > Play Southeast Asia`。
- 场景：`MaritimeLegend/Assets/Scenes/SoutheastAsiaSailingV2.unity`。
- 独立程序：`MaritimeLegend/Builds/SoutheastAsiaV2/SoutheastAsiaV2.exe`。整个目录需保留，不能只复制 EXE。
- 35° 俯视、顶部约 15% 天空；WASD 移动、右键水平旋转、滚轮缩放、R 回到南海起始视点；1/2/3 切换晴天/夕阳/星夜。
- 镜头移动边界已扩展到区域地图，最大观察距离 560。

## 美术规则

- 星空“再加 2 倍”按原数量加两份解释：约 3 倍总密度；每颗星的 Shader 亮度系数为原来的 4 倍。对应 Sky Shader 的 Star Density=3、Star Brightness=4，可继续调节。
- 普通山体和山顶使用一致的暖黄色岩土基色，不再普遍加深棕色帽子。
- 仅高度超过 15 且离岸超过 18 世界单位的部分开始向深黄色过渡；高度 24、离岸 36 时达到完整深色。高度及海陆距离都参与判定。
- 东南亚不套用旧测试岛群的北部雪山规则，避免把热带岛屿变成统一雪山。
- 地形高度从插画色彩与海岸距离推导，是游戏美术地形，不是实测高程。

## 性能与验证

960×540 世界范围；1254×706 高度/海岸/植被数据；180 个地形块，各两级 LOD。
所有块近景地形合计 159,506 三角形，远景合计 43,344；共享树石实例 7,338 个，使用 531 个分区批次进行视锥剔除。
实例复用 V2 模型，不为每棵树创建 GameObject；海面保持两个三角形。

Unity 6000.3.10f1 / Built-in / D3D11 构建成功。主工程 Input System 分支也通过 C# 编译检查。
`Review/performance.txt` 是 1600×900 强制离屏渲染并每帧 GPU 同步回读的测试，不是交互窗口 FPS。九组平均约 3.4–11.9 ms/帧。
报告沿用通用测试标签：near-35 对应南海，far-north-35 对应马六甲广角，horizon-35 对应菲律宾。

`Review/map-overview.png` 为俯视全图检查，实际游玩保持 35°。其余预览：south-china-sea、malacca、philippines、night-stars。

## 重新生成

1. `python ArtSource/ManualSailingV2/SoutheastAsia/prepare_terrain.py`（numpy/scipy/Pillow）。
2. Unity 菜单 `Create Southeast Asia`；它会先更新共享 V2 素材和原测试场景，再生成独立区域场景。
3. 独立构建方法：`MaritimeLegend.Editor.SoutheastAsiaV2Study.Build`。

当前仍是可运行的程序化地形原型，未替换成最终精细手绘 Tiles，未接入船舶碰撞、港口或存档航行系统。

## 窗口、海浪与船队修订

新版可执行程序：`MaritimeLegend/Builds/SoutheastAsiaV2-WavesShips/SoutheastAsiaV2.exe`。
默认启动 1920×1080 Windowed，运行日志已确认该状态；保留旧程序目录，避免影响仍在运行的旧版本。
按 F 拉近起始船队，R 返回区域默认视距。加入游戏现有 junk / square / lateen 三种八方向船图：复用原图、轻微浮动、随相机旋转切换朝向；当前用于视觉展示，尚未接入操船逻辑。

海浪采用六组不同方向、非整数倍波长与深水色散速度的正弦叠加，加上连续空间扰动与局部振幅变化。参考 NVIDIA GPU Gems Chapter 1 的 sum-of-sines 思路：
https://developer.nvidia.com/gpugems/gpugems/part-i-natural-effects/chapter-1-effective-water-simulation-physical-models
这是像素法线/光照波浪，未声称实现几何 Gerstner 位移或 FFT。
移除按世界网格取样的颗粒纹理和规则横纵相乘的焦散；细节按像素覆盖范围淡出。

近景实图：`Review/fleet-close.png`、`Review/sunset-close.png`。
新一轮性能结果：`Review/WavesShips/performance.txt`，固定 1920×1080 离屏渲染并同步 GPU。
该报告的第一行 Screen Resolution 在启动的 SetResolution 生效前采样；最终窗口已在 `waves-ships-runtime.log` 确认为 1920×1080 Windowed。离屏目标始终为 1920×1080，不受该启动时序影响。

## 柔和水面修订

最新 EXE：`MaritimeLegend/Builds/SoutheastAsiaV2-SoftWater/SoutheastAsiaV2.exe`。
针对用户截图的密集碎亮点：移除两层最细波，四组波向收拢到共同主风向；主波长 18→26、法线强度 0.42→0.30，减弱空间扭曲。
高光由窄亮斑改成宽而低强度的连续反光，减少随机明暗切割；岸边白浪保留。
已检查晴天侧视、正视与夕阳的 Unity 实际预览，Shader 检查及 Windows 构建通过。
本次未重新执行性能基准，旧性能报告仅对应之前版本。
