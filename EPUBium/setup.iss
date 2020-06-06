; 脚本由 Inno Setup 脚本向导 生成！
; 有关创建 Inno Setup 脚本文件的详细资料请查阅帮助文档！

#define MyAppName "EPUBium"
#define MyAppVersion "1.0.0.1"
#define MyAppPublisher "ZYFDroid"
#define MyAppExeName "EPUBium.exe"

[Setup]
; 注: AppId的值为单独标识该应用程序。
; 不要为其他安装程序使用相同的AppId值。
; (生成新的GUID，点击 工具|在IDE中生成GUID。)
AppId={{F01247F5-0B1C-4729-8632-A8C75227F7E1}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Debug\README.txt
OutputDir=D:\用户文件\桌面
OutputBaseFilename=EPUBCSharp_setup.exe
SetupIconFile=D:\用户文件\桌面\EPUBCSharp\EPUBium\ic_book.ico
Compression=lzma
SolidCompression=yes

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\EPUBium.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\cef.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\cef_100_percent.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\cef_200_percent.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\cef_extensions.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.BrowserSubprocess.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.BrowserSubprocess.Core.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.BrowserSubprocess.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.BrowserSubprocess.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.Core.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.Core.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.WinForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.WinForms.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.WinForms.XML"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\CefSharp.XML"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\chrome_elf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\d3dcompiler_47.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\debug.log"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\devtools_resources.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\eBdb.EpubReader.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\EPUBium.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\EPUBium.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\EPUBium.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\icudtl.dat"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\Ionic.Zip.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\libcef.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\libEGL.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\libGLESv2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\Newtonsoft.Json.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\README.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\snapshot_blob.bin"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\v8_context_snapshot.bin"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\locales\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\swiftshader\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\用户文件\桌面\EPUBCSharp\EPUBium\bin\x64\Release\webres\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; 注意: 不要在任何共享系统文件上使用“Flags: ignoreversion”

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

