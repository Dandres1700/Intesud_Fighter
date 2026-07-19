#define MyAppName "Intesud Fighter"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Diego Zurita"
#define MyAppExeName "INTESUD_FIGHTER.exe"

#ifndef MyBuildDir
  #define MyBuildDir "..\Builds\Windows"
#endif

#ifndef MyOutputDir
  #define MyOutputDir "..\dist"
#endif

[Setup]
AppId={{8D529517-FC57-4E52-9209-C73F629DEFD8}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Instalador de {#MyAppName}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
AppCopyright=Copyright (C) 2026 Diego Zurita
DefaultDirName={localappdata}\Programs\Intesud Fighter
DefaultGroupName=Intesud Fighter
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#MyOutputDir}
OutputBaseFilename=Intesud-Fighter-Setup-v{#MyAppVersion}-Logo
SetupIconFile=IntesudFighter.ico
WizardSmallImageFile=..\Assets\Texture\Fondo Inicial\Logo_Juego.png
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
WizardSizePercent=110
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications=yes
RestartApplications=no
SetupLogging=yes

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos adicionales:"; Flags: unchecked

[Files]
Source: "{#MyBuildDir}\*"; DestDir: "{app}"; Excludes: "INTESUD_FIGHTER_BurstDebugInformation_DoNotShip\*"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Intesud Fighter"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\Intesud Fighter"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Ejecutar Intesud Fighter"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent
