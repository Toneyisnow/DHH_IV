# 北欧 3D 港镇实景

依据用户在 2026-11-08 再次提供的港口设计图，重做空间布局并以真实 3D 建筑替代此前 V2 平面建筑卡片。

## 布局对应

- 中央石板街从近景延伸到远端港区，保留主要视线通道。
- 右前方是两层商人主楼，带完整坡屋顶、阳台、门阶、四面门窗和烟囱。
- 左侧为云杉、桦树及两组市场摊位；近景两侧植被形成遮挡。
- 后方错落布置红木民居、赭色民居、市政厅和独立远码头建筑。
- 水域位于街区两侧及远端，右侧远景停靠高桅船，左右泊位各有小船。
- 右侧近水面布置栈桥和露出船骨架的修船区。

保留参考图的相对位置与前后层次，建筑材质转为北欧红木墙、浅色木饰、灰色坡屋顶和石基座。该场景是独立的美术实景，不修改现有存档或重新分配城市 ID。

## 真 3D 的范围与证据

四种建筑模型（场景中五栋实例）都由封闭墙体核心、实心屋顶、实体窗框/门框/阳台/烟囱组成，四面都有立面细节。没有使用整栋建筑的贴图卡片或 SpriteRenderer。

树木、市场、船体、栈桥、桶箱、修船骨架和远岛同样由网格构成。中央人物保留已有欧洲角色的独立停止帧，飞鸟沿用已有鸟素材；人物与飞鸟属于 2D 角色/效果，不能据此把整座城市说成纯 3D 人物场景。未使用被用户否定的 Nordic NPC V1 步态。

验收包括：所有顶点有限；建筑 X/Y/Z 范围；六个方向的表面法线；实体部件有向体积；建筑内无 SpriteRenderer；透视相机 10 米与 30 米处单位宽度投影比为 3。`Review/geometry-checks.txt` 记录实际数值。侧面、背面与绕行视频展示真实视角变化。

## 文件与运行

- Unity 场景：`MaritimeLegend/Assets/Scenes/NordicHarborScene.unity`。
- 游戏模型：`Assets/Resources/Models/Cities/Nordic/Harbor3D/`。
- 材质与预制体：对应 `Materials`、`Prefabs/Cities/Nordic/Harbor3D/`。
- 完整组合：`NordicHarborLayout.prefab`。
- 可编辑模型：本目录 `Models/*.obj`、`*.mtl`；`NordicHarbor3D.blend` 保存整座城镇的几何与基础色。
- Blender 源场景保留真实几何和基础色；最终像素材质、场景光照与景深由 Unity 实现，不能把 Blender 基础色视图当作最终效果。
- 构建主源：`Workflow/NordicHarbor3DBuilder.cs`。构建脚本将其复制到 Unity Editor 目录作为执行入口；美术修改应以本目录版本为准。

打开场景后 Play：**1** 回到参考构图，**2 / 3** 切换左右斜视；按住右键拖动绕场景旋转，滚轮缩放。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ArtSource/NordicHarbor3D/Workflow/Build.ps1 -Player
```

`-Player` 同时生成 `MaritimeLegend/Builds/NordicHarbor3D/NordicHarbor.exe`，并运行独立场景检查。它不会启动正式游戏 UI 或触碰玩家存档。

`Reference.png` 是用户提供的参考，`Review/Reference.png` 是 Unity 正视渲染，`Review/Runtime-Reference.png` 是独立程序运行截图。两者来源不同。

## 本次验证结果

2026-11-08：Unity 独立程序构建及运行检查通过；五栋建筑的实体网格与透视检查通过。Blender 3.5 成功导入完整布局并保存可编辑源文件。`Review/Orbit.mp4` 为 6 秒视角移动预览；正视和侧视运行截图分别为 `Runtime-Reference.png`、`Runtime-Side.png`。

构建日志在覆盖旧版修船骨架资产时记录过旧网格的 NaN 警告；替换后的全部场景顶点已通过有限值检查，独立程序运行日志无该警告。当前视觉为低多边形几何配像素材质，与参考图的精细手绘表现仍有差距。

### 明亮日照调整（2026-11-08）

按用户反馈提亮红木、赭色墙面、木饰、灰瓦和树冠底色；环境补光由 (0.31, 0.36, 0.42) 提升到 (0.42, 0.48, 0.57)，阳光强度由 1.2 提升到 1.8，阴影强度由 0.91 提升到 0.96。增加直射光相对于环境光的强度，让受光面明亮、背光面仍有结构；推远海雾并减少镜头雾化，保留清晰的近景明暗关系。
