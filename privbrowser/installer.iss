; PrivBrowser installer script
; Compile with Inno Setup 6 (ISCC.exe) — see .github/workflows/build.yml,
; which compiles this automatically on every push.

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
PrivilegesRequired=admin
OutputDir=Setup files
OutputBaseFilename=PrivBrowser-Setup
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
WizardStyle=modern
SetupIconFile=app.ico
WizardImageFile=wizard_large.bmp
WizardSmallImageFile=wizard_small.bmp
LicenseFile=privacypolicy.txt

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
; --- Step 2: Welcome screen ---
WelcomeLabel1=Welcome to PrivBrowser
WelcomeLabel2=Welcome to PrivBrowser. It's privacy-focused and has an ad blocker, tracker blocker, and support for using your own VPN in PrivBrowser.%n%nClick Continue to keep going, or Close to exit Setup.
ButtonNext=&Continue
ButtonCancel=&Close

; --- Step 3: Privacy policy screen (uses LicenseFile above) ---
WizardLicense=Privacy Policy
LicenseLabel=Please read the following privacy policy before continuing.
LicenseLabel3=I accept the privacy policy. By continuing, you agree to how PrivBrowser handles your data as described above.
LicenseAccepted=I &accept the privacy policy
LicenseNotAccepted=I do &not accept the privacy policy

; --- Step 4: Install location screen ---
WizardSelectDir=Install here
SelectDirLabel3=Setup will install PrivBrowser into the following folder.
SelectDirBrowseLabel=To continue, click Continue. If you'd like to choose a different folder, click Browse.

; --- Step 5: Ready / confirmation screen ---
WizardReady=Ready to Install
ReadyLabel1=Do you really want to install PrivBrowser now?
ReadyLabel2a=Click Install to continue, or click Back if you want to review or change any settings.
ButtonInstall=&Install

; --- Step 6: Installing progress screen ---
WizardInstalling=Installing PrivBrowser
StatusExtractFiles=Installing PrivBrowser, please wait...

; --- Step 7: Finished screen ---
FinishedHeadingLabel=Thanks for installing PrivBrowser!
FinishedLabel=Thanks for installing PrivBrowser — we hope you like our browser!
FinishedLabelNoIcons=Thanks for installing PrivBrowser — we hope you like our browser!
ButtonFinish=&Finish

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
