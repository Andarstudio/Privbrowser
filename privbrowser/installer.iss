[Setup]
AppName=PrivBrowser
AppVersion=1.0.0
AppPublisher=PrivBrowser
DefaultDirName={autopf}\PrivBrowser
DefaultGroupName=PrivBrowser
OutputDir=Output
OutputBaseFilename=PrivBrowserSetup
Compression=lzma2/max
SolidCompression=yes

; Custom Icons
SetupIconFile=app_icon.ico
UninstallDisplayIcon={app}\PrivBrowser.exe

; Windows UAC Prompt ("Allow control of this device?")
PrivilegesRequired=admin

; Privacy Policy Page
LicenseFile=privacypolicy.txt

; Allow user to choose custom folder
DisableDirPage=no
DisableProgramGroupPage=yes

[Messages]
; Welcome Page Custom Text
WelcomeLabel1=Welcome to PrivBrowser
WelcomeLabel2=PrivBrowser is safe, private, and lightweight. It features built-in ad-blocking, a privacy shield, and lets you use your own private browser.%n%nClick Next to continue, or Cancel to exit Setup.

; Ready / Confirmation Page
ReadyLabel1=Do you want to install PrivBrowser on your computer?
ReadyLabel2=Click Install to begin the installation, or click Back to review your settings.

[Files]
; Packages everything from your published build folder
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Creates Desktop and Start Menu shortcuts
Name: "{group}\PrivBrowser"; Filename: "{app}\PrivBrowser.exe"
Name: "{autodesktop}\PrivBrowser"; Filename: "{app}\PrivBrowser.exe"

[Run]
; Finish Page checkbox to launch the app
Filename: "{app}\PrivBrowser.exe"; Description: "Launch PrivBrowser"; Flags: postinstall nowait skipifsilent
