# Character05：非洲男性分层行走验证

最新预览为 [FitV3](FitV3.md)：保留正面眼白、白色头巾后绳独立后脑图层。运行 `MaritimeLegend/Builds/Character05LayeredWalkFitV3/Character05LayeredWalk.exe`。

最新修正为 [FitV2](FitV2.md)：稳定正面眼鼻嘴、修正披风肩颈/领口对位和非对称方向、头饰佩戴比例与头发遮罩。当前检查输出217张视图；以下首轮记录保留作为制作背景。

使用 `$maritime-pixel-walker` 的流程，以现有非洲男性服装/配饰概念板为身份和款式参考。当前是首次换装原型，供用户检查动作和穿戴效果。

## 使用

运行 `MaritimeLegend/Builds/Character05LayeredWalk/Character05LayeredWalk.exe`。WASD/方向键移动，R回中心；左侧切换服装颜色、披风、帽子、武器、鹦鹉和工具包，可逐方向逐帧检查或自动循环。

Unity 场景与代码位于 `MaritimeLegend/Assets/Character05PixelWalk/`，Prefab 位于 `Assets/Resources/Prefabs/Characters/Character05/LayeredPixelWalker.prefab`。菜单 `Maritime Legend/Character 05 Layered Walk/Open Preview` 打开场景。

## 本次实现

|维度|已制作选项|
|---|---|
|衣服|8套协调配色：象牙/沙色、靛蓝/金、沙色/墨、鼠尾草/棕、铁锈/棕、灰/炭、赭黄/棕、石板/锈红|
|披风|短棉披、长棉披、条纹单肩披、贸易丝披、夜行长披、交叉肩巾，共6款|
|头饰|H1白头巾、H3棉帽、H5棕榈帽，共3款，另可露发|
|挂饰|A1刀、A2剑、A3鹦鹉、A4航海工具、A5贸易工具、A6甲板工具|

刀剑共用右腰槽；鹦鹉独立左肩槽；工具包独立左腰槽。包含各槽的“无”后，8×7×4×3×2×4=5376种配置。并没有烘焙5376套人物。

每个图集320×768，单格64×96，8方向各4行走+1停止。衣服使用三材质通道换色，装备是同尺寸透明图层，随相同方向和帧号选择。披风分前后片，手在需要时盖住装备；刀剑位置与人物自身右侧绑定。

## 来源与复现

- `catalog-reference.json`：原始角色概念信息；`prompts.json`：生成提示词；`Source/`：最终采用的五方向基础、帽子、披风和挂饰源图。
- `MaritimeLegend/Assets/Character05PixelWalk/Editor/LayeredWalkSetup.cs`：裁切、归一化、镜像/相位、配色、挂点、合层、资源导入和构建参数。
- 编辑器菜单 Rebuild 或批处理 `MaritimeLegend.Editor.LayeredWalkSetup.BuildAndVerify` 生成资产与预览。批处理用独立测试项目，避免关闭用户正在使用的Unity工程。
- 可执行文件加 `-layeredWalkSmoke -batchmode` 执行验证，并在 Review 输出截图和结果。

## 检查证据

`Review/runtime-validation.txt` 记录5376组合、129024次方向/姿势选择，以及移动、停止、移除装备、头部配色隔离检查。126张实际渲染视图汇总为基础八方向、全装备八方向、8色搭配、装备选项、披风前后五张图。

实测发现并修正同臂重复、背面方向错误和侧面鹦鹉挡脸；具体经验已回写技能的 `references/character05-test.md`。

## 当前边界

这是分层结构和美术迁移验证，不是全部组合已经人工验收的量产成品。头饰先测试3款，尚非原方案全部8款；装备摆动采用挂点与像素形变，未逐件精绘40帧；披风/前景手区域遮罩仍可细化。历史年代细节未独立考证。极端帽檐、兜帽、长羽毛和持物动作可能需要发型遮罩或基础局部变体。

头饰、挂饰通常可以增量加入，不必重画完整基础行走图；但必须适配每个方向、帧和遮挡关系。现有Character04烘焙搭配不能直接声称已经分层，此测试以独立Character05实现，不修改已验收V14。
