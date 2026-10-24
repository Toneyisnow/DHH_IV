印度洋手动航行 — 2026-10-22

快速测试
双击 MaritimeLegend/Builds/ManualSailing/Play-Manual-Sailing.cmd。
这会使用独立的试航存档，直接从印度沿海随机港外进入地图。
首次进入需要生成分块地形与合批植被，请等待几秒。

正式入口
启动同目录 MaritimeLegend.exe → 进入城市 → 码头/港口 → 手动航行。
自动航行入口继续保留。

操作
左键点击海面：改变目的方向，到点后继续前行。
右键左右拖动：围绕船队水平旋转，俯角固定 40°；上下拖动不改变镜头。
滚轮：拉近/拉远。空格：停船/继续。W/S：速度。A/D：帆角。R：帆角复位。
三艘船沿航向排列；镜头以中央 20% 区域跟随船队。
海岸阻挡并支持低速切向滑动；船只上下浮动。

地图
地理轮廓来自 indian_ocean.png，海岸距离场平滑后裁切三角形；海岸与海面齐平。
五类地表：浅黄沙滩、金黄沙漠、青绿草地、深绿草地、雪地。
114 处独立山体、664 处小型沙丘，10200 株三尺寸/三色松树。
非沙漠山脚随机植树，20 扇区统计最低覆盖率 70%。
松树沿用 CoastalStudy；山体为新建的 11 类 IndianOcean FBX/Prefab，保留哑光渐变、深色描边和山脚透明融合。
海水深浅来自原画颜色与离岸距离的游戏化推算，并非真实测深数据。

当前范围
随机出发点：果阿、卡利卡特、科钦、马苏利帕特南。
这些是印度洋地图港口锚点，尚未新增对应的城内建筑或贸易场景。
回到出发水域可结束本次试航，返回出航前的城市；紧急救援沿用原规则。
本次没有扩充旧世界城市目录，也没有迁移已有经济/任务存档。

验证
Unity Windows 构建成功。
MANUAL_SEA_PASS：四个出生点安全、四个旋转角度下点击投影往返、到点继续航行、航向保持、停船、海岸扫掠碰撞。
HARBOR_FLOW_PASS：城市进港、返回城市、自动航行入口、补给不足拦截、手动进入印度洋、保持原游戏状态。
美术检查图与日志：ArtSource/ManualSailing/IndianOcean/。
生成入口：python ArtSource/ManualSailing/IndianOcean/build_map.py
运行时生成数据：MaritimeLegend/Assets/Resources/Environment/IndianOcean/。
SAILING_1016_CHECKS_PASS：日历/补给/事件/存档兼容等现有航行规则回归通过。

2026-10-22 镜头参数及地形扩展
进入手动航行后，在 Hierarchy 选中 SailingScene 对象，在 Sailing Scene 组件中调整：
Minimum Zoom = 32（最近缩放；值越小越近）
Maximum Zoom = 620（最远缩放；值越大越远）
Pitch = 40（俯角，支持 10～85 度）
Play 模式调整立即生效，包括停船或暂停时；退出 Play 后按 Unity 默认行为恢复。
要长期保留参数，请在非 Play 模式调整 SailingScene.unity 并保存场景。
鼠标右键仍只改变水平角度；点击投影会使用新的俯角与缩放值。

山体资源：
Angular_Small / Medium / Large：棱角山脉，连峰、V 型山谷与硬棱。
Rolling_Small / Medium / Large：低缓山脉，连续平滑坡面。
Snow_Small / Medium / Large：对应 Angular 的同一几何，加不规则雪顶。
Dune_Small：小型风成沙丘，运行时底部贴合地面。
Cliff_Large：陡壁、台地与顶部高峰的大型悬崖山。
FBX：MaritimeLegend/Assets/Resources/Models/Environment/IndianOcean/
Prefab：MaritimeLegend/Assets/Resources/Prefabs/Environment/IndianOcean/
可编辑源文件：ArtSource/ManualSailing/IndianOcean/IndianOcean_Landforms.blend
生成脚本：先用 Blender 执行 build_landforms.py，再用 Python 执行 build_map.py。
Unity 菜单 Maritime Legend/Import Indian Ocean Landforms 可重新生成 Prefab。

沙地细节使用密度分区、缓起伏风纹、可变沙滩宽度和独立沙丘；临水区域保持平缓。
新增验证：运行中修改缩放边界与俯角、3 种俯角下的点击投影、11 类模型已布置及大中小尺寸顺序。
