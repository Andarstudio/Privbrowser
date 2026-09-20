#define MyAppName "PrivBrowser"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Andar Studio"
#define MyAppExeName "PrivBrowser.exe"

[Setup]
AppId={{B8F5E2A1-5B47-4E4C-9D4B-PRIVBROWSER}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}

DefaultDirName={autopf}\PrivBrowser
DefaultGroupName=PrivBrowser

OutputDir=..\InstallerOutput
OutputBaseFilename=PrivBrowser-Setup

Compression=lzma
SolidCompression=yes

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

PrivilegesRequired=admin
WizardStyle=modern

DisableProgramGroupPage=yes

[Files]

Source: "..\publish\*"; \
    DestDir: "{app}"; \
    Flags: ignoreversion recursesubdirs createallsubdirs

Source: "..\InstallerFiles\MicrosoftEdgeWebView2Setup.exe"; \
    DestDir: "{tmp}"; \
    Flags: deleteafterinstall

[Icons]

Name: "{autoprograms}\PrivBrowser"; \
    Filename: "{app}\{#MyAppExeName}"

Name: "{autodesktop}\PrivBrowser"; \
    Filename: "{app}\{#MyAppExeName}"

[Run]

Filename: "{tmp}\MicrosoftEdgeWebView2Setup.exe"; \
    Parameters: "/silent /install"; \
    StatusMsg: "Installing Microsoft Edge WebView2 Runtime..."; \
    Check: WebView2Missing; \
    Flags: waituntilterminated

Filename: "{app}\{#MyAppExeName}"; \
    Description: "Launch PrivBrowser"; \
    Flags: nowait postinstall skipifsilent

[UninstallDelete]

Type: filesandordirs; \
    Name: "{localappdata}\PrivBrowser"

[Code]

function WebView2Missing: Boolean;
var
  Version: String;
begin
  Result := True;

  if RegQueryStringValue(
    HKLM64,
    'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F1A5A3E8-9D9F-4E7E-B5A1-9A4F5B5D7E2C}',
    'pv',
    Version
  ) then
  begin
    if Version <> '' then
      Result := False;
  end;

  if Result then
  begin
    if RegQueryStringValue(
      HKLM32,
      'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F1A5A3E8-9D9F-4E7E-B5A1-9A4F5B5D7E2C}',
      'pv',
      Version
    ) then
    begin
      if Version <> '' then
        Result := False;
    end;
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;

  if CurPageID = wpWelcome then
  begin
    if not IsWin64 then
    begin
      MsgBox(
        'PrivBrowser requires 64-bit Windows.',
        mbError,
        MB_OK
      );

      Result := False;
    end;
  end;
end;
