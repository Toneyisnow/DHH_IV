# Imagegen 修复输入与提示词记录

模式：内置 imagegen，transparent_background=true。参考均先本地查看。原始生成返回文件已复制到本目录。每个文件对应独立请求，没有使用 FrameRonin/API key。

## SW-equipment-edit.png
输入 SW-motion-guide.png。提示重点：精确保留镜像自已批准SE的各帧肢体姿态、四个不同相位；frame1近臂后/近腿前，frame3近臂前/近腿后；仅去除被镜像到近侧的腰包、恢复衣料，正确腰包在人物右侧远侧。透明4格一行，不生成新动作。

## W-equipment-edit.png
输入 W-motion-guide.png。提示重点：精确保留镜像自已批准E的动作；frame1近臂向屏幕右后摆、近脚向左；frame3近臂向屏幕左前摆、近脚向右后；两个通过帧保持不同腿。仅修近侧错误腰包，纯正左侧视图。

## NE-attempt-rejected.png
输入 NE-before.png 和 cartoon-walk-A-v1.png（第三排SE作为运动参照）。请求四个不同的反向手脚相位，明确近臂frame3应后摆、近腿前迈，两个passing必须不同。输出仍然重复1/3，故接触帧弃用；最终仅采用列2和4的不同过渡姿势。

## NE-contact-arm-only.png
输入旧 NE-01.png。单帧提示：屏幕右肩当前向右上伸出的手臂改为向左下后摆，另一臂向右上前伸；双腿交换前后，勿保留旧臂形成第三只手。头、衣装、视角保持。结果手臂正确但腿连接仍需要修正，未作为最终单帧。

## NE-contact-opposite.png
输入 NE-contact-arm-only.png。局部下半身提示：只改红腰带以下，保留上半身；近侧右臂向左下后摆，因此腰包正下方的近侧右腿必须朝右前方迈出，最右鞋露鞋面；披布下远侧左腿朝左下后伸，露鞋底。交换裤腿的连接和遮挡，不只是换鞋颜色。

## NW-equipment-edit.png
输入 NW-motion-guide.png（修正NE四帧的镜像运动参考）。保持各自四个不同姿态，尤其第3帧近臂后摆/近腿前迈，第1帧相反。去除近侧错误腰包，恢复衣料；人物实际右侧包在远侧隐藏。背对左上，不能回头露正脸。

## W-idle-equipment.png
输入旧 W-idle.png。局部移除近侧腰包并恢复蓝披布/米白衣料；保持停姿、双脚落地、手臂下垂、头脸、尺度、透明底。最终只合成腰包的小区域，其他像素保留旧停止图。

后处理：repair-gait05.cjs清理透明杂点、按旧图高度与头中心对齐；publish-gait05.cjs保留头部70px及未修方向、合成W停止帧局部；finish-04-05.cjs 05重新同步整图与图层。
