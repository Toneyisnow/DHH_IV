# 亚裔女性侧向低抬脚修正 v3

用户反馈正左/正右抬腿角度不自然。本次只改 E/W 的第2、4过渡帧的下裙摆与鞋：高抬膝、向前踢脚、后折脚改为支撑脚旁低离地通过，保留两次不同的鞋前后重叠。前后接触帧、摆臂、头部、其余6方向与全部停止帧不变。

内置imagegen原始编辑为 E-passing-low.png，提示词为prompt.txt。用原E-passing.png作编辑底图；归一化后仅合成y112以下裙摆/鞋。W仅镜像对称下裙摆/鞋，非对称装备不镜像。额外清除E过渡帧旧上翘裙摆尖角，范围在y96..111、手和腰包以外；保留原上半身。

- Before/：本次修改前40帧。
- checks.json：4帧局部变化、其他36帧不变。
- side-walk.gif：E/W四帧慢速循环。
- ../Review168/、../Layers/、../walk-review-slow.gif和游戏Character04/CartoonWalk均同步。
- 重建：先运行repair-side04.cjs，再finish-04-05.cjs 04。若从更早初稿开始，先执行v2流程，再执行本次覆盖；不能只运行v2后发布。
- 城市试玩入口仍为MaritimeLegend/Builds/CityPreview/Play-Asian-Female.cmd。

程序检查说明范围与资源完整性；步态效果另以低速四帧目视审阅。城市运行检查在腿部修正后进行，最后裙摆尖角清理只改变接缝小区域；最终构建包含清理。

低抬脚版本城市运行检查通过：CITY_ART_PLAYMODE_PASS，未发现FAILED/Exception。UnityReview保存E/W第2、4帧实际渲染，截图取自最终裙摆尖角清理之前；低抬脚部分与最终版一致。final-build.log为包含尖角清理的最终构建记录。
