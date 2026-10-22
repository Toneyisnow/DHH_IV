亚裔女性八方向像素行走验证

Unity 菜单：Maritime Legend > Character 04 Pixel Walk > Play Preview。
场景：Assets/Character04PixelWalk/Character04PixelWalk.unity
Prefab：Assets/Resources/Prefabs/Characters/Character04/PixelWalker.prefab
WASD / 方向键移动；组合按键斜向移动；1/2 换搭配；R 回到中心。
左侧面板支持八种上衣配色、自动行走、方向预览、逐帧冻结、动画速度与镜头缩放。

规格：每套 8 方向 x（4 行走帧 + 1 独立待机帧）；单格 64x96；图集 320x768；Point 过滤。
图集由上到下：S、SW、W、NW、N、NE、E、SE；每行五帧，前四帧循环，第五帧为双脚着地待机。
角色脚底为坐标原点，32 pixels per unit；斜向速度归一化。
资源通过 SpriteRenderer 展示，首次选择搭配/配色生成缓存。

本次验证使用两套烘焙搭配；披风、挂饰尚未拆成可独立组合的动画层。
配色为上衣区域的颜色阈值遮罩原型，不是最终逐像素美术遮罩。
AI 源图经透明裁切、统一比例缩放及颜色量化得到游戏图集；仍需要最终像素修整。
这是独立移动预览，未接入主游戏寻路、碰撞或存档。

源图保存在本目录；Review 保存标准化图集、Unity 渲染与验证记录。
仅游戏可用图集和 Prefab 放入 Unity Resources；没有改动主游戏场景。
编辑器重建：MaritimeLegend.Editor.PixelWalkSetup.BuildAndVerify。
独立预览启动参数 -pixelWalkSmoke 可运行换装、移动和渲染验证并退出。



当前版本 V7：V6 修正交替摆臂，V7 进一步修正左右腿的前后阴影与遮挡。源图在 V7/；运行截图在 Review/unity-v7-diagonal-cycles.png（行：红衣 SW、红衣 SE、披风 SW、披风 SE；列：四个行走帧、停止帧）。
预览支持 Loop this direction in place 原地循环；可降低帧率并冻结第 1、3 帧对比。自动检查仅验证像素、播放和移动，动作是否自然仍需视觉审阅。


V8：修正 NW 多余袖口/手臂；增强 NW 右臂、NE 左臂摆动并调整遮挡。使用内置 imagegen，提示词保存于 V8/prompts.txt。Unity 实际逐帧图 Review/unity-v8-rear-cycles.png，行顺序为红衣 NW、红衣 NE、披风 NW、披风 NE。


V9：NE 在运行时水平翻转已确认正确的 NW 动作，并偏移半个周期，确保左右手脚相位一致。图集 NE 原始行不再被角色使用；其余方向保留 V8。当前为烘焙搭配，翻转也会影响非对称服装细节，后续独立挂饰层需单独处理左右侧。详见 Review/v9-workflow.txt。


V10：将 NW 镜像及半周期相位偏移直接烘焙到最终图集 NE 行，取消运行时翻转替代。最终 Resources 图集与 Unity 播放一致；V8/NE.png 是历史错误源图，不再作为 NE 最终结果。检查 Review/unity-v10-rear-cycles.png 或 Resources 中 base-walk.png、voyage-walk.png。导入检查逐像素验证 NE 是对应 NW 的镜像。


V11：S、SW、SE 三个方向各自复用首个行走帧的头部层（单格 y=60..95，Unity 底部为原点），应用于四个行走帧及停止帧；两套搭配均如此。颈部以下像素保持既有动作。自动校验这些方向的头部可见像素/透明度逐帧一致。


V12：撤销 V11 整头固定；S、SW、SE 仅稳定四个行走帧的眼睛与嘴部局部矩形（FacePatches）。其余头发像素恢复原始逐帧变化；独立停止帧保留原来的闭嘴造型。

V12 局部表情区域随每帧头部位置对齐，保留头部位移和区域外头发变化。

V13：S 停止帧眼部沿用行走帧并对齐头部位置；NW 眼神改为面向行进方向，NE 镜像同步。仅合成生成源图的眼部区域。提示词与源图保存于 V13/，眼部放大对照为 Review/eyes-v13-closeup.png（S/NW/NE，每套三行）。


V14：收窄 SW/SE 行走腿部间距，仅替换单格 y<22 的四个行走帧；其余像素与 V13 一致。内置 imagegen 源图及提示词在 V14/；像素范围校验及腿部宽度对比在 Review/leg-spacing-v14.txt。

