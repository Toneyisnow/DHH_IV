# 商品像素资源

当前商品表共 52 种。使用内置 image_gen 生成，最终提示词见 prompts.json；图集与商品 ID 对照见 catalog.json。review.html 直接展示 Unity Resources 内的最终独立 PNG。

游戏资源位于 MaritimeLegend/Assets/Resources/Sprites/Commodities/，按稳定商品 ID 命名。Unity 编辑器菜单 Maritime Legend / Publish Commodity Pixel Sprites 可从本目录重新发布，验证全部商品覆盖、透明通道与每格内容。Sprite 使用 Point 过滤、无 mipmap、无压缩。

造型区分：原木保留树皮和圆形年轮，木板为直边锯切板，涂层木材为深色涂层板；铁矿和铜矿为粗糙岩石，铁锭和铜锭为规则金属锭；亚麻为茎秆与纤维，棉花为棉铃，羊毛为卷曲毛束；原石保留矿物基质，切割宝石为抛光几何形；颜料为干粉，高级染料为瓶装液体。

所有图集保留原始生成尺寸与透明通道。发布只按格子切分，不重绘、不用色键抠图。品质星级由交易界面绘制，不烘焙进商品图片。
