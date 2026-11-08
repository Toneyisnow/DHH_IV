# FrameRonin 接入核实 · 2026-11-07

用户询问后，已通过真实网页界面检查官网、全局 API 设置、完美八方向模块和官方使用说明；尚未向 FrameRonin 上传角色或调用生成。

先前全部母图与行走试稿使用内置 image_gen。不能标为 FrameRonin 结果。现有 Character06/Review128 与 Review168 为 40 张切分审阅图，各含四帧行走和一帧停止；`walk-review-slow.gif` 为慢速预览，未通过全部解剖检查或 Unity 验证。其余七角色仅归档参考图，未制作动画。

## 实际看到的界面

- 全局设置：API Key、API 模式、Host、Model、激活 API。
- API 模式下拉：Legacy / custom async API、OpenAI Compatible chat completions、OpenRouter chat completions、工作室模式、代充模式。
- 默认模型显示 nano-banana-fast，不代表其他服务具有此模型名称。
- 完美八方向：上传角色，复杂模式、成品样式、非像素、不抠图、纯侧面、60 度俯视；生成按钮显示“生成朝向图（1~2次）”，另有自由序列入口。
- 官方说明：生成需要可用 Key 和对应权限；代充不等于全站通行证。普通模板需要激活 API；建议先生成一份样本检查按钮消耗。

## 仍待核实

可用提供方、实际模型 ID、余额及模块权限；完整生成调用尚未进行。UI 支持某模式不等于该服务所有模型可用于八方向生成。未确认独立公开八方向 REST API。

密钥由用户在设置界面或本地凭据位置配置，不写入此仓库或聊天记录。

官方入口：https://frameronin.com/ · https://frameronin.com/tut
