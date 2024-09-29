# MoreFlyout

## 开发日志

### v1.1.2-health action1-2024.09.22

- 此版本主要目的为修复程序目前存在的错误，优化程序结构，同时完善日志。
- 修复了Shell主程序打开时，主页的按钮始终处于关闭状态的bug。

### v1.1.1-2024.09.16

- 现在修复了程序启动时所有按钮默认处于关闭的bug。

### v1.1.0-2024.07.24

- 删除C++构建的MoreFlyout.Shell，改为C#编写。
- 统一MoreFlyout.Server和MoreFlyout.Shell的 .Net、Windows App SDK版本。
- 新增Preview分支，作为预览更新，在更新完毕时并入main分支，Preview分支可能会采用预览版Window App SDK，在使用预览版时不会并入main分支。

### v1.0.0-2024.06.01

- 在分支CppVersion中曾考虑将主程序改为C++重构，但经过多次的压力测试，不得不面对C++ WinUI程序长时间运行下极易出现的内存泄露问题，决定保留主程序为C#编写，同时删除托盘图标功能，将项目分为MoreFlyout.Server和MoreFlyout.Shell，同时MoreFlyout.Shell部分由C++编写，作为程序的可视化管理外壳。

### v0.0.3-beta3-2024.03.12

- 更改了程序结构，精简之前由TemplateStudio自动生成的模板，同时更改了部分命名，例如：MainWindow改为FlyoutWindow更加直观。精简了部分逻辑代码。
- 感谢[Simon Mourier](https://github.com/smourier)的[回答](https://stackoverflow.com/questions/78210920/in-c-sharp-winui-a-crash-about-system-executioninexception-is-caused-for-unk)，成功解决了因为CG回收导致WinUI的严重崩溃。
- 将整个窗口显示改为Flyout组件显示，虽然系统Flyout其实没有使用WinUI Flyout的动画，但是动画效果上已经很接近了。
- 将Windows App SDK由1.4版本升级到1.5版本，更改了整个程序的退出方法，不再因为窗口的崩溃导致整个程序的崩溃，后续考虑将检测窗口状态的功能放入托盘图标。
- 程序必须以管理员方式运行，才能够显示在其他应用上层。
- 添加了其他窗口全屏检测的功能，在有其他窗口全屏时（包括全屏的游戏窗口），Flyout不会显示。
- 删除了托盘图标功能，在搜寻多方内容后，Windows App SDK不提供直接的托盘图标似乎也是一种无奈，如今的程序对用户的干扰入侵越来越深，托盘图标似乎并不是用户一定想拥有的。但是此程序本身由于没有直接的窗口界面，托盘图标是用户得知程序运行状态与设置功能的一个良好的入口。同时隐藏托盘后，用户没有直接的设置窗口再次启用托盘功能。最终决定在接下来的更新中重新加入托盘，且不可隐藏。
- 完善了语言文件。
- 添加托盘图标功能，加入开机自启选项。
- 将Windows App SDK升级到1.5.240404000。

### v0.0.2-beta2

- 修复了在高DPI情况下窗口位置、大小错位的问题。
- Acrylic效果单独创建，避免窗口在失焦的情况下变为纯色效果。
- 添加托盘图标功能。

## 更新日志

### v1.1.1-2024.09.18

1. 原程序变更为MoreFlyout.Server，作为基础服务部分，删除了托盘图标的功能。
2. 新增MoreFlyout.Shell，作为程序的可视化管理设置外壳，同时同步Server与Shell的Windows App SDK和.Net版本。
3. 修复了在v1.0.0版本开始存在的：Shell主程序打开时，主页的按钮始终处于关闭状态的bug。

- 经测试，第3点并未成功修复，将在下一个版本修复。😭

### v0.0.2-beta2-2024.03.11

1. 现在能够提供正常的Acrylic效果。
2. 现在程序能够正常在托盘图标显示。

### v0.0.1-beta1-2024.02.26

1. 基础功能更新。
