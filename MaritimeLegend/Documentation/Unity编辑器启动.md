# 从 Unity Editor 运行

工程根目录：`D:\SourceCode\Git\toneyisnow\DHH_IV\MaritimeLegend`

Unity Hub 中 Add → Add project from disk，选择上述目录（这一层同时有 Assets、Packages、ProjectSettings），使用 Unity 6000.3.10f1 打开。旧素材目录 D:\PlayGround\大航海时代复刻 不是 Unity 工程。

等待导入完成，菜单 **Maritime Legend → Open Game Scene** 打开入口场景，然后按顶部 Play；也可菜单 **Maritime Legend → Play Game**。入口场景为 `Assets/Scenes/MainMenuScene.unity`。已设置编辑器 Play 默认从此场景启动。

编辑状态的场景主要是相机，游戏 UI 在 Play 时由代码创建。切换到 Game 标签即可看到标题。

若提示项目已被另一个 Unity 打开，应返回已经打开的 MaritimeLegend 编辑器实例；先保存并关闭旧实例后才能重新打开。不要在编辑器运行时删除 UnityLockfile。
