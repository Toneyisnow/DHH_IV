# 六地域沿海元素 V2

本版细化道路、草坪、民居、市政厅、树种和树形，新增浮动停泊船与船坞，并把鸟改为 1–2 秒快速跨屏的随机小群。

完整规范：[城镇沿海元素V2与布局规范](../../../MaritimeLegend/Documentation/城镇沿海元素V2与布局规范.md)。

- `Originals/<Region>/Expansion.png`：各地域 12 元素原图；每套增加第二种民居、市政厅、两艘船、六种树形、修船平台、半成品船。
- `Originals/GrassAtlas.png`：六地域草地材质原图。草坪三种边界由可编辑网格生成。
- `Originals/Africa/Expansion-before-palm-fix.png`：修正前版本，仅供追溯；游戏使用修正后的下垂枯叶棕榈。
- `Workflow/prompts.json`：内置 image_gen 完整提示词和修正记录。
- `Review/`：Unity 动态渲染与验收。

在仓库根目录执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ArtSource/RegionalCityElements/Workflow/Build-Library.ps1 -Version2
powershell -NoProfile -ExecutionPolicy Bypass -File ArtSource/RegionalCityElements/Workflow/Encode-Review.ps1 -Version2
```

游戏使用 `Assets/Resources/Prefabs/Cities/<Region>/V2/`。每套包含道路 6、草坪 3、房屋 3、树木 6、市场 1、两船 2、修船平台与半船 2、码头与船坞组合 2、鸟大小和编队 3、水域 1、沿海组合样例 1，共 30 个预制体。

验收已通过：180个V2预制体；道路2米接口和旋转；三种草坪几何；草坪高度低于道路；码头/船坞水域占地；两船保持水平锚定并浮动；飞鸟编队数量、过屏时间、随机方向与三种画面比例。详见 Review/PASS.txt、checks.txt 和 build.log。

Review/SixStyles.mp4 为实际Unity渲染的六地域8秒动态总览，每地域另有单独视频。保留00、10、95帧与总览首帧；中间帧可通过构建脚本重新生成。
