# NPC 制作规范 · 2026-11-08修订

## 美术层级

NPC显著朴素于主角，保留卡通画风与人物身份，降低服装装饰量和发型复杂度。普通亚麻、粗羊毛、旧皮革；每套最多两个主要布料色。无刺绣、金边、花纹披肩、成排亮金属扣、珠饰、复杂编发冠。服装变化靠剪裁、厚薄、围裙/短外衣/素色短披布来区分，不只是变色。M1可无披风，M2用普通保暖外衣或单片披布。

北欧四人简化方案：

|人物|H1|H2|M1|M2|
|---|---|---|---|---|
|成年男性|普通短发|简单后束短马尾|素色亚麻衬衣、褐色膝裤、简单腰绳|灰蓝粗羊毛短外衣、普通裤袜，木扣或布绳|
|成年女性|简单低发髻|单条低辫|灰褐工作裙、素亚麻围裙|暗蓝工作裙、无花纹短披布|
|老人|稀疏短白发|简单后梳及颈白发|灰褐旧上衣、普通膝裤|厚羊毛短外衣、无纹围巾|
|小孩|普通短发|一条简单低辫|灰蓝素裙、普通鞋袜|赭褐素裙、短羊毛背心|

保持各自脸型、年龄、体型、发色和身份；不要借简化衣服重造人物。以上是下一版规格，尚非已经生成的素材。

## 概念编辑prompt模板（尚未执行）

Edit the approved full-body NPC reference. Preserve identity, face silhouette, age, proportions, pose, hair color and painted game-art style. Simplify ONLY hair and clothing into an ordinary 1600s Nordic town resident, visibly much plainer than a protagonist. [H option] and [M option]. Plain coarse wool and linen, maximum two main cloth colors, simple cloth ties or one wooden fastening. No embroidery, gold trim, patterned mantle, decorative brooch, rows of shiny buttons, jewelry, elaborate curls or braided crown. Keep full head and both feet visible. Do not copy the old ornate outfit.

## 四帧相位

以角色自己的左右命名，不能混淆屏幕左右：

|帧|腿和支撑|双臂|
|---|---|---|
|1 接触A|右脚前方落地，左腿后伸|左臂前摆、右臂后摆|
|2 通过A|右脚承重；左膝和左脚从后方向前通过，左脚离地|左臂由前向后经过髋旁，右臂由后向前经过髋旁|
|3 接触B|左脚前方落地，右腿后伸|右臂前摆、左臂后摆|
|4 通过B|左脚承重；右膝和右脚从后方向前通过，右脚离地|右臂由前向后经过髋旁，左臂由后向前经过髋旁|

上/下方向将解剖左右映射到正确屏幕侧。侧面必须明确近侧肩属于哪只胳膊，沿肩—肘—腕追踪，不能凭前方出现一只手判定换臂。严格正侧面可能存在遮挡，遮挡不构成通过证据。

## 动作prompt模板（尚未执行）

Use reference1 ONLY for the four verified skeleton poses, reference2 ONLY for NPC identity and plain clothing. Transfer each respective pose exactly; do not copy a standing pose or repeat a half-cycle. Generate ONE direction, FOUR equally spaced full-body frames on one transparent strip. Direction: [N/S/W/E; explicit screen-facing and anatomical-side mapping]. Frame1: right foot forward in contact, left leg back; left arm forward and right arm back. Frame2: right foot planted supporting weight, left knee and foot passing forward off ground; left arm retreats through hip, right arm advances through hip. Frame3: left foot forward in contact, right leg back; right arm forward and left arm back. Frame4: left foot planted supporting weight, right knee and foot passing forward off ground; right arm retreats through hip, left arm advances through hip. Trace each hand to its own shoulder, each shoe to its own hip. Two arms, two legs, one grounded support at all times, relaxed narrow walk, no march or jump. Distinct passing poses must exchange support leg. Stable identity and scale, actual transparent alpha. Stop frame is a separate later asset, not included in these four.

若整条仍重复，只重做失败接触/通过帧，使用前一已验收帧及骨架对照；不能继续靠加重形容词重复整图生成。基础四相位通过后才生成衣装与发型变化。

## 后期验收门槛

1. 每方向保留1/3并排、2/4并排、原尺寸四帧、放大四帧和2–4fps循环。记录输入图、实际prompt、采用/弃用版本。
2. 对两条臂链、两条腿链逐帧写明前/后、支撑/通过。第1/3未换领先侧直接失败；第2/4未换支撑侧直接失败。不可用脚的颜色或位置偏移代替骨架交换。
3. 循环按1→2→3→4→1查看，确认没有半周期重复、滑步、第三只手、贴地平移、突然缩头/换脸。未知或遮挡不明标记unverified，不能自动pass。
4. 用头/躯干锚点统一格子，不把每张脚底最高的摆动脚当支撑脚，避免裁切归一化抹掉真实起伏。
5. 正面统一眼鼻嘴局部母版；脸之外的头发仍可有轻微自然运动。
6. 所有换装逐方向复查，不能由基础一套通过推定其他套通过。最后才输出Resources并进行Unity实际移动验收。

结构检查与动作检查分开记录。非空/尺寸/alpha/hash检查只标structuralChecks；gaitApproved必须有上述目视和循环证据。
