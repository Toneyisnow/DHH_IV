# 第二轮：五官、披风与头饰贴合

根据用户指出的首版问题修改，而非把首版自动测试通过视为视觉验收。

可运行预览：`MaritimeLegend/Builds/Character05LayeredWalkFitV2/Character05LayeredWalk.exe`。Unity场景和Prefab已同步为修正版。旧预览目录仅保留作比较。

- 正向下：在最终64×96格内，复用停止帧的脸内部母版（局部x27..36、y64..73，原点在左下）。眼、鼻、嘴和邻近肤色一致，范围外头发仍来自各动作帧。八配色五帧逐像素检查。
- 披风：从各帧基础图的肩部服装区域定位肩颈，领口跟随肩颈；收小肩宽和长度，保留轻微下摆摆动。M3单肩、M5叠襟长披、M6交叉肩巾使用独立8方向源图，不镜像右侧。M3固定人物自身左肩，NW近侧在身体前，NE远侧在身体后，允许被躯干遮挡。
- 头饰：尺寸按头部而非服装外框调整；每帧对齐头部，降低帽口到发际线。新增戴帽专用头发剔除版本 hatted-body-C1..C8，去掉帽内上部头发；帽口中心开口让脸露出，无帽恢复完整头发。原始 body-C1..C8 不受剔发影响。

非对称披风使用内置imagegen生成，源图为 Source/capes-asymmetric-v2.png，提示词见 fit-v2-prompt.json。其余修正由现有Unity导入/合成管线执行。独立对照：Review/front-face-five-frames.png、headwear-fit-eight-directions.png、cape-rear-three-directions.png。披风图每行一款，每组三列是同一方向的第1、第3、停止帧；三组依次NW、N、NE。

首版截图保存在 Review/BeforeFitV2/。目前仍为待用户检查版本；自动像素、图层检查不代表所有姿势穿插均通过美术验收。
