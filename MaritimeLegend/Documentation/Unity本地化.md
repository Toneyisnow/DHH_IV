# Unity 五语言配置

依据《航海世家_1016版补充设计_纯设计架构版》第六章（首发语言支持）：简体中文 `zh-CN`、English `en-US`、繁體中文 `zh-TW`、日本語 `ja-JP`、한국어 `ko-KR`。源语言和缺失译文回退语言均为简体中文。不包含五语配音。

## 查看和修改翻译

1. 用 Unity 6000.3.10f1 打开项目，等待 Package Manager 解析官方 `com.unity.localization` 1.5.13 及其 Addressables 依赖。
2. 打开 **Window > Asset Management > Localization Tables**，选择 **GameText**。每行是稳定 Key，每列是一种语言，可直接编辑译文。也可用 **Maritime Legend > Localization > Open translation tables** 打开。
3. **Edit > Project Settings > Localization** 查看五个 Locale、源语言、系统语言选择器及简体中文最终回退配置。
4. 运行游戏后在设置页底部选择语言，即时更新已接入的文本并写入全局用户设置。Unity Game 视图的 Locale 选择器也可预览已接入文本；预览切换本身不写入用户设置。

原生表为 Unity 运行时的权威文本来源。`Resources/Localization/*.json` 保留为初始导入和独立领域测试的种子数据；编辑原生表后无需同步 JSON 才能在游戏生效。**Create or repair five languages** 仅补建缺失 Locale/表/Key，不覆盖已有译文，也不会自动删除旧 Key。新增文本先加入种子文件，再执行该菜单，或直接在原生表创建。

Locale、String Table Collection 与 Localization Settings 位于 `Assets/Localization/`；Unity 官方 Localization 用 Addressables 注册 Locale 和翻译表，因此这些原生配置必须在 `Resources` 外，避免同时使用 Resources 与 Addressables 管理同一资产。种子资源继续保留在既定 `Resources/Localization/`。`Assets/AddressableAssetsData/` 为 Unity 生成的寻址配置，需连同表及 `.meta` 一起提交。

## 当前覆盖与边界

- 五语各 43 个 Key：原有 21 个航海事件/海域文本，以及新增 22 个主菜单、设置页文本。数字占位符使用 `{0}`、`{1}`、`{2}`，由语言文化信息格式化；这些表项不启用 Smart Strings。
- 已接入主菜单按钮、设置页和原有 `SeaText` 调用。未接入的商品名、城市、任务、剧情、其他界面及图像内文字仍保持原文；本次不是全游戏五语翻译完成。
- 新装时按系统语言选择，无法匹配则回退简体中文；已保存的全局语言优先。尚未接入 Steam 语言 API。
- Windows 界面字体候选补充 Yu Gothic 和 Malgun Gothic。未完成可离线分发字体、全部中日韩字形及逐页布局的验收；主菜单专用美术字体仍需独立检查。
- 繁体、日文、韩文新增译文待人工术语和语气复核。`translationsComplete` 与各 JSON `complete` 保持 `false`。

## 检查

**Maritime Legend > Localization > Validate five languages** 验证五语 Locale/Key 一致性、非空译文、占位符一致性与格式化、运行时读取、修改原生表立即生效、空译文/未知语言回退及未知 Key 标记。结果写入 `Documentation/LocalizationChecks.txt`。不覆盖全部游戏流程、字体显示或 Steam 检测。

官方参考：[Unity Localization 1.5](https://docs.unity3d.com/Packages/com.unity.localization@1.5/manual/index.html)。
