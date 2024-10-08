; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "MoreFlyout"
#define MyAppVersion "1.1.2"
#define MyAppPublisher "ChenYiLins"
#define MyAppURL "https://github.com/ChenYiLins/MoreFlyout"
#define MyAppExeName "MoreFlyout.Shell.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{ED91AC41-325B-4665-9660-328538C260E0}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
PrivilegesRequired=none
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
UsePreviousAppDir=no
LicenseFile=D:\Projects\Source\CSharp\MoreFlyout\LICENSE
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=D:\Downloads
OutputBaseFilename=MoreFlyout
SetupIconFile=D:\Projects\Source\CSharp\MoreFlyout\Assest\InstallIcon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "D:\Projects\Source\Package\MoreFlyout\MoreFlyout\MoreFlyout.Shell\{#MyAppExeName}"; DestDir: "{app}\MoreFlyout.Shell"; Flags: ignoreversion
Source: "D:\Projects\Source\Package\MoreFlyout\MoreFlyout\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\MoreFlyout.Shell\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName:"MoreFlyout.Server"; Flags: uninsdeletevalue
