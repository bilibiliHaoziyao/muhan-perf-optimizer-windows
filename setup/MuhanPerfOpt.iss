; ========================================================================
; 慕寒性能优化 - Inno Setup 安装脚本
; 输出: MuhanPerfOpt-Setup-<VERSION>.exe
; 发布模式: SelfContained + SingleFile (用户无需 .NET Runtime)
; ========================================================================

#define AppName        "慕寒性能优化"
#define AppPublisher   "Muhan"
#define AppId          "{1A2B3C4D-5E6F-7890-ABCD-EF1234567890}"
#define AppVersion     "74.1.0"
#define AppExeName     "MuhanPerfOpt.exe"
#define AppIcon        "..\src\MuhanPerfOpt\Assets\icon.ico"
#define PublishDir     "..\publish\win-x64"
#define OutputBaseName "MuhanPerfOpt-Setup"

[Setup]
; ---------- 基本信息 ----------
AppId={{#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/bilibiliHaoziyao/muhan-perf-optimizer-windows
AppSupportURL=https://github.com/bilibiliHaoziyao/muhan-perf-optimizer-windows/issues
AppUpdatesURL=https://github.com/bilibiliHaoziyao/muhan-perf-optimizer-windows/releases
DefaultDirName={autopf}\MuhanPerfOpt
DefaultGroupName={#AppName}
OutputDir=output
OutputBaseFilename={#OutputBaseName}-{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#AppExeName}
SetupIconFile={#AppIcon}
PrivilegesRequired=admin
MinVersion=10.0.17763
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; ---------- 安装程序元数据 ----------
SetupMutex={{#AppId}-Setup}
RestartIfNeededByRun=no

; ========================================================================
; 语言
; ========================================================================
[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english";     MessagesFile: "compiler:Default.isl"

; ========================================================================
; 任务：桌面快捷方式、开机自启
; ========================================================================
[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加图标:"; Flags: unchecked
Name: "startupicon"; Description: "开机自动启动"; GroupDescription: "启动选项:"; Flags: unchecked

; ========================================================================
; 文件
; ========================================================================
[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#AppIcon}";      DestDir: "{app}"; DestName: "icon.ico"; Flags: ignoreversion
Source: "..\LICENSE";      DestDir: "{app}"; DestName: "LICENSE.txt"; Flags: ignoreversion

; ========================================================================
; 图标/快捷方式
; ========================================================================
[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\icon.ico"
Name: "{autodesktop}\{#AppName}";  Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\icon.ico"; Tasks: desktopicon

; ========================================================================
; 运行注册表：开机自启（可选任务）
; ========================================================================
[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; \
    ValueType: string; ValueName: "MuhanPerfOpt"; \
    ValueData: """{app}\{#AppExeName}"" --silent"; \
    Tasks: startupicon; Flags: uninsdeletevalue

; ========================================================================
; 安装后运行
; ========================================================================
[Run]
Filename: "{app}\{#AppExeName}"; Description: "立即运行 {#AppName}"; Flags: nowait postinstall skipifsilent
