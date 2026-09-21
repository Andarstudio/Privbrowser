; PrivBrowser installer script
; Built automatically by GitHub Actions using Inno Setup (pre-installed on
; windows-latest runners). Output: installer\Setup files\PrivBrowser-Setup.exe

#define MyAppName "PrivBrowser"
#define MyAppVersion "1.0"
#define MyAppPublisher "Andar Studio LLC"
#define MyAppExeName "PrivBrowser.exe"

[Setup]
AppId={{A4C2E1F8-3B5D-4E9A-8C7F-PRIVBROWSER01}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\PrivBrowser
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Setup files
OutputBaseFilename=PrivBrowser-Setup
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
Source: "..\privbrowser\publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
