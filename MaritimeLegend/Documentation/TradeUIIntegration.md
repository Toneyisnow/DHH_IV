# 港口交易所

入口：城市原有“市场”按钮。原表格市场已由原生 Unity uGUI 交易面板替换，1920×1080 参考画幅，保留固定布局与独立滚动区域。返回城市将丢弃未成交草稿。

## 数据和兼容

- 城市供货从当前城市 market 按品质读取，报价使用 GameService.Trade/Quote 的真实逐件供需算法。结算在克隆状态内完成，全部通过后才替换当前状态并自动保存。
- 等级 0 在真实数据中是 S，因此显示特殊星形；1–5 显示相应星数。未改变品质数值。
- 旗舰继续使用原来的 GameState.cargo；其他船新增 Ship.cargo。旧存档不会把旗舰货物重新分配到其他船。运输中的船只显示航线货物并禁止交易。
- Inventory.cargoCells 仅记录位置。每次打开按实际 batches 校正，因此生产、委托、调拨等已有系统仍以 batches 为准，不会凭位置缓存复制货物。每份一个货仓；原有重量容量限制继续生效。
- 仅当当前城市已有产业园许可证时开放仓库交易。先售出再买入，允许复用腾出的货仓和卖出收入。原有任务奖励仍由 GameService 触发。
- 市场打开时暂停世界推进，防止交易过程中后台供需改变。提交仍校验库存、金币、城市和船只状态，变化时整单拒绝。
- 新增 trade_olive_oil 和 trade_pottery，追加于商品顺序末尾，不改变原 ID、任务商品索引或旧档品质。旧城市缺失的新商品市场记录初始为零。

## 测试货物

## 货仓布局与交互（2026-10-25）

- 船只 cargoSlotCount 默认 3，在交易界面限定为 3–6；每艘船的一行固定预留六个位置，未开放位置不可购买。产业园开放 12 个货仓，六列两行。
- 旧存档超过新限制的已有货物标为“待整理货物”，继续保留并可卖出、撤销卖出；这些额外位置不接收新购商品，整理后不再保留空白溢出位置。原有重量上限继续生效。
- 商品和货仓 hover 上移 6 个逻辑像素，显示金色描边；不缩放商品图片。飞行只显示固定 188×100 的边框，使用独立子 Canvas，保持 0.323 秒直线移动和并发点击。
- 面板复用 RectTransform、Image、Text 和 Button；刷新时替换监听器而不是叠加。取消每次操作与落地时全量 Destroy/AddComponent。报价复用本次交易预览的 GameService，不再为每张卡片初始化游戏服务。
- TradeSessionChecks 覆盖默认容量、六格上限、满货仓拒绝购买和旧库存保留。TradeUiSmoke 检查市场卡片实例复用、无内容飞行框、hover 提起和成交。Tools/TradeUiCompile 可在编辑器占用时编译运行时代码；最终视觉和运行验证仍以 Unity 为准。

仅在 Unity Editor 或 Development Build：首次打开市场时，旗舰各补一份缺少的小麦、橄榄油、羊毛、陶器，品质 4。原有货物保留；载重不足则不添加。完成后写入 trade-ui-test-cargo-v1 标记并自动保存，后续不重复补货。正式非开发构建不触发此逻辑。

## 美术

- 编辑源：仓库根目录 ArtSource/CommodityPixelArt/，catalog.json 记录图集与商品 ID，prompts.json 保存内置 imagegen 生成提示词。
- 游戏商品资源：Assets/Resources/Sprites/Commodities/{商品ID}.png，覆盖当前完整商品表的 52 种原料、加工品与成品。
- CommoditySpritePublisher 在编辑器中按图集格子确定性拆分，检查覆盖率与透明通道，导入独立 Sprite；Point 过滤、无 mipmap、无压缩。交易界面通过 Resources.Load<Sprite> 按商品 ID 读取。
- 原木使用带树皮圆木，木材使用锯切板材；铁矿、铜矿使用不规则矿石，铁锭、铜锭使用规则金属锭。亚麻与棉花、原石与切割宝石、干颜料与液体染料分别绘制。
- 里斯本本地原料 p13 是亚麻，p14 才是棉花。旧 POC 八格示意图保留在 ArtSource/TradeUI_POC，仅作为设计过程记录。
- 港口背景：Assets/Resources/Trading/harbor-background.png，源图和提示词保存在 ArtSource/TradeUI_POC/。

## 验证

HeadlessChecks 包含 TradeSessionChecks：草稿隔离、混合交易、货仓位置持久化、金币不足、旧存档库存、仓库权限、测试种子幂等及状态变化拒绝。Unity 构建入口 MaritimeLegend.Editor.TradeUiBuild.Build。开发运行标志 -tradeUISmoke 仅使用复制的测试档案，截图写入 Evidence/TradeUI。

布局压缩：城市商品卡 180×220，左上角库存、右上角星级，供货区完整显示两行；待售区固定 110 高（含悬浮留白），商品单行排列，仅横向滚动。底部两按钮均为 196×58，金额、状态提示与按钮位于同一行。
