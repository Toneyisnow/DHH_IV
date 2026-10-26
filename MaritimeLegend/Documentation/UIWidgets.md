# 通用 uGUI widgets

代码目录：`Assets/Shared/UI/Widgets/`。这些组件不读取城市、船只或交易状态，调用者负责传入数据与点击回调。

- `WidgetFactory`：传入矩形创建函数、字体、正文色和强调色。创建函数可以接现有对象池；同一个 GameObject 上的组件会被复用。
- `CardWidget.Create`：创建带 Button 的卡片，绑定点击回调及可交互状态。无标题卡片使用通用 hover 上提和描边；带标题时可作为普通按钮。
- `StockCardWidget.Render`：传入 `StockCardData`（图片、名称、等级文字及颜色、价格、库存、变暗状态）。支持城市供货竖卡和图片在左的横卡；null 数据显示空货仓。文字由调用方本地化后传入。
- `PanelWidget.Scroll`：传入可视矩形、内容长度和滚动方向，返回 content 与 scroll。固定方向滚动，滚动条 AutoHide，不因显示/隐藏改变布局。所有滚动面板绘制 2px 黑色内边框。
- `PanelWidget.Border`：可单独用于非滚动面板。`PanelBorder` 只生成四条边，不填充面板中央，不拦截点击。
- `WidgetScrollRect`：横向面板可响应普通鼠标纵向滚轮及触控板横向滚动，始终固定 Y；无溢出时不移动。

交易界面通过自己的池化 Box 函数构造 WidgetFactory，将商品状态转换为 StockCardData。Widget 内不执行报价、买卖或保存。

当前布局：城市供货区 700 高，可完整显示三行 220 高卡片及间距；待售区 110 高、单行横滚；底部按钮 196×48，位于 y=1018，距 1080 屏幕底边 14 像素。默认交易提示小字已移除，失败原因与成交反馈仍按需显示。
