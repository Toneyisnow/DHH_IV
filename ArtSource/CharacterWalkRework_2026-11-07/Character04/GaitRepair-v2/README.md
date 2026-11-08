# Character04 八方向步态修复 v2

本次按用户要求重做亚裔女性全部8方向的32张行走帧，保留大头卡通身份、短发、红色交领上衣、黑金长裙和黑鞋。非裔男性资源不变。

## 动作处理

- N/S：分别生成左右脚交替的正背面步态。两只鞋保持独立窄轨迹；接触帧1/3交换前脚与前摆臂，过渡帧2/4交换支撑脚与抬起脚。
- E/W/SE/SW/NE/NW：以非裔男性已修正的相应四相位作为运动参考，将角色替换为亚裔女性设计。未采用最初E尝试，该尝试仍重复1/3，保留在E-rejected.png供追溯。
- E/SE/SW：两个过渡帧再单独制作，明确支撑脚、抬跟与通过的膝脚；不能用相同姿势仅移动几像素。
- W：上半身独立制作，只在两个过渡帧复用镜像的对称下裙摆/鞋区域（y>=112）。衣襟、脸、腰包没有直接镜像。
- N：修正头颈接缝，按原上衣领口定位下半部并维持脚底基线；不移动头部。
- 每个行走帧上方64px沿用原图，包含既有头发、五官及其轻微起伏。正面继续共用停止帧五官母版。
- 8个停止姿势保留；仅W停止帧的小块腰包区域(x87..101,y93..114)修正遮挡，避免停走时错误出现近侧腰包。

## 交付

- ../Review168/walk-sheet.png：最终840×1344透明图集，168×168单格；N NE E SE S SW W NW行序，0..3行走，4停止。
- ../walk-review-slow.gif：4fps慢速八方向循环。
- contact-review-1.png：N/NE/E/SE四帧对照。
- contact-review-2.png：S/SW/W/NW四帧对照。
- ../Layers/head.png、body.png、clothing.png：同步更新的三层；复合与整图无差异。仍不是任意换装全集。
- Before/：修复前40帧；FinalFrames/：修复后的逐帧来源。
- prompts.json：全部内置imagegen调用的实际提示词；输入运动参考为*-motion.png，身份为../mother-S-cartoon.png。未使用FrameRonin或外部key。

游戏资源同步至MaritimeLegend/Assets/Resources/Sprites/Characters/Character04/CartoonWalk/walk-sheet.png。原城市启动入口MaritimeLegend/Builds/CityPreview/Play-Asian-Female.cmd继续有效。未修改存档身份或存档进度。

## 重建

在ArtSource/CharacterWalkRework_2026-11-07/运行：

1. node repair-gait04.cjs
2. node finish-04-05.cjs 04
3. 复制Character04/Review168/walk-sheet.png到游戏Character04/CartoonWalk目录，保留.meta。
4. 从Character04/Loop/frame-%02d.png以4fps生成walk-review-slow.gif。

旧pack-character04.cjs只重建初稿；不要单独运行它后发布，须继续运行上述修复流程。

## 验证说明

checks.json记录32帧改变、头部保留和局部停止帧修正。图层复合检查在../checks.json。人工审阅针对1/3帧的近侧袖口—手前后交换，以及2/4的支撑鞋/抬起鞋；正背面另核对左右脚轨迹。图片差异或程序计数器本身不能证明步态正确。

Unity检查增加8方向×4行走帧的真实城市场景截图，检查方向/格索引/锚点/Point采样及停止朝向，同时保留其他城市回归检查。完成状态与日志见UnityReview/和runtime-checks.log。

运行结果：CITY_ART_PLAYMODE_PASS；没有FAILED/Exception记录。已保存全部32帧实际渲染（另附32张带UI截图），并生成unity-contact-1.png、unity-contact-2.png作并排目视审阅。最终图集已包含全部修正，构建与运行检查均基于此版本。

后续侧向抬腿微调见 ../SideStep-v3/README.md；发布时在本页v2流程后运行repair-side04.cjs，再finish-04-05.cjs 04。最新左右方向图以v3为准。

后续最新局部修正：../.. /MotionPolish-v4（路径为 ../../MotionPolish-v4）。当前 Review168 与游戏资源已同步，旧版 FinalFrames 为历史记录。
