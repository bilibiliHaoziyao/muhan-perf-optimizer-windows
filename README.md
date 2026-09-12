# 慕寒性能优化 (Windows)

> Android 端「慕寒性能优化」的 Windows 桌面版本，基于 WinUI 3 + Fluent Design。

## ✨ 功能

- 📊 **实时硬件监控** — CPU / GPU / RAM / 电池（每秒刷新）
- 🔍 **SOC 详细面板** — 每核 CPU 利用率、GPU 温度/频率
- 🧹 **一键内存优化** — 强制归还进程工作集，降低 RAM 占用
- ⏱️ **后台自动清理** — 可配置阈值，静默运行
- 💾 **存储与磁盘** — 物理内存 + 所有分区使用情况
- 🖥️ **屏幕信息** — 分辨率、刷新率、多显示器
- ⚙️ **系统信息** — OS 版本、架构、.NET 运行时
- 🚀 **开机自启** — 通过 Windows 注册表 Run 键（免管理员）
- 📌 **系统托盘** — 常驻后台，右键快速清理

## 🛠 技术栈

| 组件 | 技术 |
|------|------|
| UI 框架 | WinUI 3 (Windows App SDK 1.6) |
| 设计语言 | Fluent Design v2 |
| 架构 | MVVM (CommunityToolkit.Mvvm) |
| 硬件采集 | LibreHardwareMonitor |
| 系统托盘 | Hardcodet.NotifyIcon.Wpf |
| 配置持久化 | System.Text.Json |
| 目标框架 | .NET 8 (win-x64, SelfContained) |
| 安装程序 | Inno Setup |
| CI/CD | GitHub Actions (windows-latest) |

## 🏗 本地构建

前置条件：Windows 10 1903+、Visual Studio 2022 或 .NET 8 SDK

```powershell
# 1. 还原依赖
cd win-app
dotnet restore

# 2. 编译
dotnet build -c Release

# 3. 运行（直接调试）
dotnet run --project .\src\MuhanPerfOpt\MuhanPerfOpt.csproj

# 4. 发布（生成自包含单文件）
dotnet publish .\src\MuhanPerfOpt\MuhanPerfOpt.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -o .\publish\win-x64
```

### 构建安装包

```powershell
# 安装 Inno Setup（winget）
winget install JRSoftware.InnoSetup

# 编译 Setup.exe
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" .\setup\MuhanPerfOpt.iss
# 产物位置: .\setup\output\MuhanPerfOpt-Setup-74.1.0.exe
```

## 🚀 发布到 GitHub Release

仓库已配置 GitHub Actions，创建 tag 自动构建：

```bash
git tag -a v74.1.0 -m "Release v74.1.0"
git push origin v74.1.0
```

也可在 Actions 页面手动触发 `workflow_dispatch`。

## 📁 项目结构

```
win-app/
├── MuhanPerfOpt.sln
├── src/MuhanPerfOpt/
│   ├── MuhanPerfOpt.csproj        # .NET 8 + WinUI 3
│   ├── App.xaml / App.xaml.cs      # 应用入口
│   ├── MainWindow.xaml             # NavigationView 主框架
│   ├── Core/                       # 业务逻辑
│   │   ├── HardwareMonitor.cs      # LibreHardwareMonitor 封装
│   │   ├── MemoryOptimizer.cs      # EmptyWorkingSet / PurgeSystemFileCache
│   │   ├── OptimizeService.cs      # 后台自动清理调度
│   │   ├── StartupManager.cs       # HKCU Run 注册表自启
│   │   ├── SettingsService.cs      # JSON 配置持久化
│   │   ├── TrayIconService.cs      # 系统托盘
│   │   └── ToastService.cs         # Toast 通知
│   ├── ViewModels/ViewModels.cs    # MVVM 所有 ViewModel
│   ├── Views/                      # 页面 (Fluent Card 风格)
│   │   ├── OverviewPage.xaml       # 概览
│   │   ├── SocPage.xaml            # CPU/GPU 详细
│   │   ├── OptimizePage.xaml       # 内存优化
│   │   ├── StoragePage.xaml        # 存储与磁盘
│   │   ├── ScreenPage.xaml         # 屏幕
│   │   ├── SystemPage.xaml         # 系统信息
│   │   └── SettingsPage.xaml       # 设置
│   ├── Resources/Colors.xaml       # Fluent 强调色
│   ├── Assets/icon.ico / icon.png  # 应用图标
│   └── app.manifest                # requireAdministrator 清单
├── setup/
│   └── MuhanPerfOpt.iss            # Inno Setup 安装脚本
└── .github/workflows/build.yml     # CI/CD 工作流
```

## 🔑 权限说明

- **管理员权限**：`app.manifest` 中配置了 `requireAdministrator`，因为：
  - 内存清理需要调用 `SetProcessWorkingSetSize`
  - 部分硬件传感器读取需要高权限
- **Windows 10 最低版本**：1903 (10.0.17763)，支持 WinUI 3

## 📜 许可证

MIT License — 见 [LICENSE](../LICENSE)。
