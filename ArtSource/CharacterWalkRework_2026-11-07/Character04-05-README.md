# Character04 / Character05 大头卡通行走图

沿用 Character06 的大头卡通风格、8 方向、4 行走帧与独立停止帧。使用内置 imagegen 生成，未调用 FrameRonin，也未使用外部 API key。

- Character04：V2 亚裔女性，黑色短发、茜红交领上衣、米白领袖、黑金长裙、腰包与玉佩。
- Character05：V2 非裔男性，束辫与米白头带、胡须、米白衣裤、红色腰带、靛蓝金边披布、凉鞋。
- 身份参考：`../CharacterArt_V2/02-Asia-Ming-v2.png`、`../CharacterArt_V2/03-Africa-v2-slim.png`；比例/风格参考为 `Character06/mother-S-cartoon-v2.png`。本次没有改动存档肖像或存档身份。

## 文件与播放

各角色目录：

- `Review168/walk-sheet.png`：840×1344，透明 PNG；单格168×168。
- 行：N、NE、E、SE、S、SW、W、NW。列0–3循环行走，列4停止。
- `Review168/*.png`：40张逐帧图。`mother-S-cartoon.png`：最终正面停止母图。
- `walk-review-slow.gif`：5fps八方向慢速审阅。
- `preview.html`：浏览器逐帧、停止与 WASD 移动预览。
- `Layers/head.png`、`body.png`、`clothing.png`：同步40帧的可编辑分层。
- `anchors.json`、`checks.json`：锚点与图层复合检查。
- `Review128` 仅为中间尺寸比较，不是最终发布资源；统一使用 Review168。

游戏资源单独存放到 `MaritimeLegend/Assets/Resources/Sprites/Characters/Character04/CartoonWalk/` 和 Character05 同名目录；旧版 PixelWalk / LayeredPixelWalker 保留。

城市试玩入口位于 `MaritimeLegend/Builds/CityPreview/`：

- `Play-Asian-Female.cmd`：亚裔女性。
- `Play-African-Male.cmd`：非裔男性。
- 原先 `Play-Cartoon-City.cmd` 仍为非裔女性，保留三款换装。

## 图层的实际范围

这次两人各完成一套原搭配。head 包含当前头发、脸与头饰；body 包含身体、其余衣服和装备；clothing 是已分离的部分主色像素（04红上衣，05蓝披布）。三层复合与最终原图逐像素一致。

可以在图像软件中打开同尺寸三层，锁定透明度，只改 clothing 的颜色；不得整体染色 head/body，否则肤色也会改变。换发型需以当前完整头部为编辑参考，另做8方向匹配的头层并保留五官、挂点和遮挡；当前头层不代表已做完无发底图。改变衣服剪裁需补画完整衣装层。不能直接把概念板裁块贴上去，也不能声称两人已经支持全部 C/H/M/A 组合。

这两人的城市入口目前播放原搭配的完整图集，分层文件用于继续编辑；06 已有的三款运行时换装保持独立。

## 重建与验证

使用 Node 执行 `pack-character04.cjs 04`、`pack-character05.cjs 05`，再执行 `finish-04-05.cjs`。后者统一正面脸部母版、输出分层及预览帧。最后将 Review168/walk-sheet.png 同步到对应 CartoonWalk 目录，保留已有 .meta。初期 prepare-pack/refine-pack 脚本仅记录制作过程，不作为重建入口。

裁切检查：40帧/人；无空格或切边报警。小块透明杂点与高饱和红黄底色残留由导入脚本剔除；05 SE第一帧扩大取图范围，避免手臂被格线切断；04 NW单独重绘为后斜视图。所有正面行走帧共用各自idle五官母版，发丝仍保留逐帧变化。头部、衣色与身体分层逐像素重组无差异。

图像仍属于可继续精修的卡通行走资源，长裙下的腿部交替可见性比裤装弱。程序帧索引检查不能证明全部肢体与装备遮挡已经美术终审。

## Unity 实测结果（2026-11-07）

Windows 城市构建成功，运行检查输出 `CITY_ART_PLAYMODE_PASS`，未发现 Exception/FAILED。新角色各40帧检查覆盖方向、索引、168像素格、脚底pivot(84,10)、Point过滤、停止后保留朝向；每个方向保存行走与停止的真实城市场景截图。截图在各角色 `UnityReview/`，完整构建/运行日志在 `Character04-05-UnityReview/`。原06三套换装和城市移动/场景回归仍通过。

运行图集与最终源图 SHA256 完全一致：

- 04：558D8DDB945256D397129B54315C91C5AAFAC4779BAC3BA61F1EC71ADF0160F4
- 05：917A4A182B2DF2CDF378603FBDFBB9AB14BE796083169D33094498179336B1A7

完整生成提示词见 `Character04-05-prompts.md`。

2026-11-07 步态修复更新：Character05 的 SW/NE/NW/W 已按用户反馈修正，见 Character05/GaitRepair-v2/README.md。该版本取代本页最初05图集及其旧SHA256；04保持原样。

2026-11-07 第二次步态更新：Character04全部8方向32张行走帧已修复，见Character04/GaitRepair-v2/README.md；该版本取代初稿04及旧SHA256。使用repair-gait04.cjs再finish-04-05.cjs 04重建，保留05的独立修复。
