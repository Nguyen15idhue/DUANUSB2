; ============================================
; DONGLE SYNC SERVICE - INNO SETUP INSTALLER
; Professional GUI Installer for Windows
; ============================================

#define MyAppName "USB Dongle Sync Service"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "CHC Geomatics"
#define MyAppURL "https://github.com/Nguyen15idhue/DUANUSB2"
#define MyAppExeName "DongleSyncService.exe"
#define MyAppServiceName "DongleSyncService"

[Setup]
; Basic Information
AppId={{8DB3F8A4-8021-4473-868A-A53BB2E39759}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Installation Directories
DefaultDirName={autopf}\CHC Geomatics\Dongle Service
DefaultGroupName=CHC Geomatics\USB Dongle Service
DisableProgramGroupPage=yes

; Output
OutputDir=F:\3.Laptrinh\DUANUSB2\output
OutputBaseFilename=DongleSyncService-Setup-v{#MyAppVersion}
; SetupIconFile=F:\3.Laptrinh\DUANUSB2\installer\icon.ico
; UninstallDisplayIcon={app}\{#MyAppExeName}

; Compression
Compression=lzma2/max
SolidCompression=yes

; Visual Style - Modern & Professional
WizardStyle=modern

; Privileges
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Windows Version Support
MinVersion=10.0
ArchitecturesInstallIn64BitMode=x64

; Misc
DisableWelcomePage=no
LicenseFile=F:\3.Laptrinh\DUANUSB2\installer\License.rtf

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Service Executable & Dependencies
Source: "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; DLLPatch.dll (Critical Component)
Source: "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish\DLLPatch.dll"; DestDir: "{app}"; Flags: ignoreversion

; Configuration Files
Source: "F:\3.Laptrinh\DUANUSB2\installer\icon.ico"; DestDir: "{app}"; Flags: ignoreversion

[Dirs]
; Create required directories
Name: "{commonappdata}\DongleSyncService"
Name: "{commonappdata}\DongleSyncService\backups"
Name: "{commonappdata}\DongleSyncService\logs"

[Icons]
; Start Menu Shortcuts
Name: "{group}\View Service Logs"; Filename: "notepad.exe"; Parameters: "{commonappdata}\DongleSyncService\logs\service-{code:GetCurrentDate}.log"
Name: "{group}\Service Manager"; Filename: "services.msc"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

; Desktop Shortcut (Optional)
Name: "{autodesktop}\USB Dongle Service Logs"; Filename: "notepad.exe"; Parameters: "{commonappdata}\DongleSyncService\logs\service-{code:GetCurrentDate}.log"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut for viewing logs"; GroupDescription: "Additional shortcuts:"

[Run]
; Optional: View service logs after installation
Filename: "{sys}\notepad.exe"; Parameters: "{commonappdata}\DongleSyncService\logs\service-{code:GetCurrentDate}.log"; Description: "View service logs"; Flags: postinstall nowait skipifsilent

[UninstallRun]
; Stop and remove service before uninstall
Filename: "{sys}\sc.exe"; Parameters: "stop {#MyAppServiceName}"; Flags: runhidden waituntilterminated
Filename: "{sys}\timeout"; Parameters: "/t 3 /nobreak"; Flags: runhidden waituntilterminated
Filename: "{sys}\sc.exe"; Parameters: "delete {#MyAppServiceName}"; Flags: runhidden waituntilterminated

[Code]
var
  CHCAppFoundPage: TOutputMsgWizardPage;
  CHCAppPath: string;
  ServiceWasRunning: Boolean;

// ============================================
// HELPER FUNCTIONS
// ============================================

function GetCurrentDate(Param: string): string;
begin
  Result := GetDateTimeString('yyyymmdd', #0, #0);
end;

function IsServiceInstalled(ServiceName: string): Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec('sc.exe', 'query "' + ServiceName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

function IsServiceRunning(ServiceName: string): Boolean;
var
  ResultCode: Integer;
  Output: string;
begin
  Result := False;
  if Exec('sc.exe', 'query "' + ServiceName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Result := (ResultCode = 0);
  end;
end;

function StopService(ServiceName: string): Boolean;
var
  ResultCode: Integer;
begin
  Log('Stopping service: ' + ServiceName);
  Result := Exec('sc.exe', 'stop "' + ServiceName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  if Result then
  begin
    // Wait for service to stop
    Sleep(3000);
  end;
end;

function DeleteService(ServiceName: string): Boolean;
var
  ResultCode: Integer;
begin
  Log('Deleting service: ' + ServiceName);
  Result := Exec('sc.exe', 'delete "' + ServiceName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  if Result then
  begin
    // Wait for service to be deleted
    Sleep(2000);
  end;
end;

function CheckCHCApp(): Boolean;
var
  CHCAppPath1: string;
  CHCAppPath2: string;
  CHCAppPath3: string;
begin
  Result := False;
  CHCAppPath := '';
  
  // Check common installation paths
  CHCAppPath1 := ExpandConstant('{localappdata}\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll');
  CHCAppPath2 := ExpandConstant('{userappdata}\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll');
  CHCAppPath3 := 'C:\Program Files\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll';
  
  if FileExists(CHCAppPath1) then
  begin
    CHCAppPath := CHCAppPath1;
    Result := True;
    Log('CHC App found at: ' + CHCAppPath1);
  end
  else if FileExists(CHCAppPath2) then
  begin
    CHCAppPath := CHCAppPath2;
    Result := True;
    Log('CHC App found at: ' + CHCAppPath2);
  end
  else if FileExists(CHCAppPath3) then
  begin
    CHCAppPath := CHCAppPath3;
    Result := True;
    Log('CHC App found at: ' + CHCAppPath3);
  end
  else
  begin
    Log('CHC App NOT found in standard locations');
  end;
end;

// ============================================
// WIZARD PAGE EVENTS
// ============================================

procedure InitializeWizard();
begin
  // Check for CHC App at startup
  CheckCHCApp();
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  ServiceInstalled: Boolean;
begin
  Result := True;
  
  if CurPageID = wpWelcome then
  begin
    
    // Check for existing service
    ServiceInstalled := IsServiceInstalled('{#MyAppServiceName}');
    ServiceWasRunning := IsServiceRunning('{#MyAppServiceName}');
    
    if ServiceInstalled then
    begin
      if ServiceWasRunning then
      begin
        Log('Service is running, will stop it');
        StopService('{#MyAppServiceName}');
      end;
      
      Log('Removing old service installation');
      DeleteService('{#MyAppServiceName}');
    end;
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  
  // Final check - stop service if still running
  if IsServiceRunning('{#MyAppServiceName}') then
  begin
    Log('Final check: Service still running, stopping now...');
    StopService('{#MyAppServiceName}');
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  ServiceBinaryPath: string;
begin
  if CurStep = ssPostInstall then
  begin
    Log('Installing Windows Service...');
    
    ServiceBinaryPath := ExpandConstant('{app}\{#MyAppExeName}');
    
    // Create service
    if Exec('sc.exe', 
      'create "{#MyAppServiceName}" ' +
      'binPath= "' + ServiceBinaryPath + '" ' +
      'start= auto ' +
      'DisplayName= "USB Dongle Sync Service" ' +
      'obj= LocalSystem',
      '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    begin
      Log('Service created successfully');
      
      // Set service description
      Exec('sc.exe', 
        'description "{#MyAppServiceName}" "Manages USB dongle authentication and DLL patching for CHC Geomatics Office 2"',
        '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
      
      // Configure service recovery
      Exec('sc.exe',
        'failure "{#MyAppServiceName}" reset= 86400 actions= restart/60000/restart/60000/restart/60000',
        '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
      
      Log('Service configuration completed');
      
      // Start service immediately
      Log('Starting service...');
      if Exec('sc.exe',
        'start "{#MyAppServiceName}"',
        '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      begin
        if ResultCode = 0 then
          Log('Service started successfully')
        else
          Log('Service start returned code: ' + IntToStr(ResultCode));
      end
      else
      begin
        Log('Failed to start service. Error code: ' + IntToStr(ResultCode));
      end;
    end
    else
    begin
      Log('Failed to create service. Error code: ' + IntToStr(ResultCode));
    end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    Log('Uninstalling: Stopping and removing service...');
    
    if IsServiceRunning('{#MyAppServiceName}') then
    begin
      StopService('{#MyAppServiceName}');
    end;
    
    if IsServiceInstalled('{#MyAppServiceName}') then
    begin
      DeleteService('{#MyAppServiceName}');
    end;
  end;
end;

// ============================================
// FINISH PAGE
// ============================================

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpFinished then
  begin
    WizardForm.FinishedLabel.Caption := 
      'USB Dongle Sync Service has been successfully installed.' + #13#10 + #13#10 +
      'The service is now running in the background and will automatically start with Windows.' + #13#10 + #13#10 +
      'Next steps:' + #13#10 +
      '  1. Create USB dongle using DongleCreatorTool' + #13#10 +
      '  2. Insert dongle to activate features' + #13#10 + #13#10 +
      'Service logs can be found at:' + #13#10 +
      'C:\ProgramData\DongleSyncService\logs';
  end;
end;
