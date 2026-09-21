; NexsusVKVideo per-user installer. Build via scripts/Build-Installer.ps1.
#ifndef AppVersion
  #error AppVersion must be supplied with /DAppVersion=<version>.
#endif
#ifndef AppSourceDir
  #error AppSourceDir must be supplied with /DAppSourceDir=<publish-directory>.
#endif

#define AppName "NexsusVKVideo"
#define AppPublisher "nvros86"
#define AppExeName "NexsusVKVideo.App.exe"
#define WebView2ClientId "{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"

[Setup]
AppId={{71D4E407-8770-44AF-8F37-1530EE101834}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={localappdata}\Programs\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\artifacts\installer
OutputBaseFilename=NexsusVKVideo-Setup-{#AppVersion}
SetupIconFile=..\src\NexsusVKVideo.App\Assets\NexsusVKVideo.ico
UninstallDisplayIcon={app}\{#AppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
DisableWelcomePage=no

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "Создать ярлык на рабочем столе"; GroupDescription: "Дополнительные ярлыки:"

[Files]
Source: "{#AppSourceDir}\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Запустить {#AppName}"; Flags: nowait postinstall skipifsilent

[Code]
var
  WebView2Page: TInputOptionWizardPage;

function IsInstalledRuntimeVersion(const Value: String): Boolean;
begin
  Result := (Value <> '') and (Value <> '0.0.0.0');
end;

function IsWebView2RuntimeInstalled: Boolean;
var
  Version: String;
begin
  Result := RegQueryStringValue(HKLM64,
    'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{#WebView2ClientId}',
    'pv', Version) and IsInstalledRuntimeVersion(Version);

  if not Result then
    Result := RegQueryStringValue(HKCU,
      'Software\Microsoft\EdgeUpdate\Clients\{#WebView2ClientId}',
      'pv', Version) and IsInstalledRuntimeVersion(Version);
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := (Assigned(WebView2Page) and (PageID = WebView2Page.ID) and IsWebView2RuntimeInstalled);
end;

procedure InitializeWizard;
begin
  WebView2Page := CreateInputOptionPage(wpSelectTasks,
    'Требуется Microsoft Edge WebView2',
    'Установщик проверит среду выполнения WebView2',
    'NexsusVKVideo использует Microsoft Edge WebView2 Evergreen Runtime. Если компонент отсутствует, можно открыть официальную страницу Microsoft после установки.',
    True, False);
  WebView2Page.Add('Открыть официальную страницу установки WebView2 после завершения');
  WebView2Page.SelectedValueIndex := 0;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ErrorCode: Integer;
begin
  if (CurStep = ssPostInstall) and (not WizardSilent) and
     (not IsWebView2RuntimeInstalled) and (WebView2Page.SelectedValueIndex = 0) then
    ShellExec('open', 'https://developer.microsoft.com/microsoft-edge/webview2/', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;
