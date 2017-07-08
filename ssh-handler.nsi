; SSH Handler
;
; Douglas Thrift
;
; ssh-handler.nsi

#   Copyright 2013 Douglas Thrift
#
#   Licensed under the Apache License, Version 2.0 (the "License");
#   you may not use this file except in compliance with the License.
#   You may obtain a copy of the License at
#
#       http://www.apache.org/licenses/LICENSE-2.0
#
#   Unless required by applicable law or agreed to in writing, software
#   distributed under the License is distributed on an "AS IS" BASIS,
#   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#   See the License for the specific language governing permissions and
#   limitations under the License.

!ifndef SSH_HANDLER_VERSION
!define SSH_HANDLER_VERSION "1.1.0.0"
!endif

!ifndef SSH_HANDLER_CONFIGURATION
!define SSH_HANDLER_CONFIGURATION "Release"
!endif

!define SSH_HANDLER_NAME "SSH Handler"
!define SSH_HANDLER_AUTHOR "Douglas Thrift"

Name "${SSH_HANDLER_NAME}"
OutFile "ssh-handler-${SSH_HANDLER_VERSION}.exe"
SetCompressor /SOLID lzma
ShowInstDetails show
ShowUninstDetails show
XPStyle on
VIProductVersion "${SSH_HANDLER_VERSION}"
VIAddVersionKey "ProductName" "${SSH_HANDLER_NAME}"
VIAddVersionKey "CompanyName" "${SSH_HANDLER_AUTHOR}"
VIAddVersionKey "LegalCopyright" "Copyright 2013 ${SSH_HANDLER_AUTHOR}"
VIAddVersionKey "FileDescription" "${SSH_HANDLER_NAME}"
VIAddVersionKey "FileVersion" "${SSH_HANDLER_VERSION}.0"
VIAddVersionKey "ProductVersion" "${SSH_HANDLER_VERSION}.0"

!define SSH_HANDLER "${SSH_HANDLER_NAME} ${SSH_HANDLER_VERSION}"
!define SSH_HANDLER_EXE "ssh-handler.exe"

!define SWC_INPUT_TOOLKIT_DLL "System.Windows.Controls.Input.Toolkit.dll"
!define SWC_LAYOUT_TOOLKIT_DLL "System.Windows.Controls.Layout.Toolkit.dll"
!define WPF_TOOLKIT_DLL "WPFToolkit.dll"

!define DOT_NET_FRAMEWORK_EXE "dotNetFx40_Client_setup.exe"

!define UNINST_REG "Software\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)"
!define UNINST_EXE "ssh-handler-uninst.exe"

!define MULTIUSER_EXECUTIONLEVEL "Highest"
!define MULTIUSER_INSTALLMODE_COMMANDLINE
!define MULTIUSER_INSTALLMODE_DEFAULT_REGISTRY_KEY "${UNINST_REG}"
!define MULTIUSER_INSTALLMODE_DEFAULT_REGISTRY_VALUENAME "UninstallString"
!define MULTIUSER_INSTALLMODE_INSTDIR "${SSH_HANDLER_NAME}"
!define MULTIUSER_INSTALLMODE_INSTDIR_REGISTRY_KEY "${UNINST_REG}"
!define MULTIUSER_INSTALLMODE_INSTDIR_REGISTRY_VALUENAME "InstallLocation"
!define MULTIUSER_MUI

!include "MultiUser.nsh"
!include "MUI2.nsh"
!include "FileFunc.nsh"

!define MUI_COMPONENTSPAGE_NODESC
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_SHOWREADME "https://github.com/douglaswth/ssh-handler/blob/${SSH_HANDLER_VERSION}/README.md"
!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE LICENSE
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MULTIUSER_PAGE_INSTALLMODE
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!define MUI_UNFINISHPAGE_NOAUTOCLOSE

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

!macro REGISTRY BASE_KEY
    WriteRegStr ${BASE_KEY} "Software\Classes\ssh" "" "URL:SSH Protocol"
    WriteRegStr ${BASE_KEY} "Software\Classes\ssh" "URL Protocol" ""
    WriteRegStr ${BASE_KEY} "Software\Classes\ssh\shell\open\command" "" '"$INSTDIR\${SSH_HANDLER_EXE}" "%1"'
    WriteRegStr ${BASE_KEY} "${UNINST_REG}" "DisplayName" "$(^Name)"
    WriteRegStr ${BASE_KEY} "${UNINST_REG}" "DisplayVersion" "${SSH_HANDLER_VERSION}"
    WriteRegDWORD ${BASE_KEY} "${UNINST_REG}" "EstimatedSize" $0
    WriteRegStr ${BASE_KEY} "${UNINST_REG}" "Publisher" "${SSH_HANDLER_AUTHOR}"
    WriteRegStr ${BASE_KEY} "${UNINST_REG}" "UninstallString" "$INSTDIR\${UNINST_EXE}"
    WriteRegStr ${BASE_KEY} "${UNINST_REG}" "InstallLocation" "$INSTDIR"
    WriteRegDWORD ${BASE_KEY} "${UNINST_REG}" "NoModify" 1
    WriteRegDWORD ${BASE_KEY} "${UNINST_REG}" "NoRepair" 1
!macroend

!macro UN_REGISTRY BASE_KEY
    DeleteRegKey ${BASE_KEY} "Software\Classes\ssh"
    DeleteRegKey ${BASE_KEY} "${UNINST_REG}"
!macroend

Section /o ".NET Framework 4 Client Profile" dot_net_framework_index
    SectionIn 1 RO
    SetOutPath $TEMP
    File ${DOT_NET_FRAMEWORK_EXE}
    ClearErrors
    ExecWait '"$TEMP\${DOT_NET_FRAMEWORK_EXE}" /passive /norestart /chainingpackage "${SSH_HANDLER_NAME}"' $0
    IfErrors Failed
    Delete ${DOT_NET_FRAMEWORK_EXE}
    IntCmpU $0 0 Done
    IntCmpU $0 1641 RebootFlag
    IntCmpU $0 3010 RebootFlag
Failed:
    Abort "Failed to install .NET Framework 4 Client Profile!"
RebootFlag:
    SetRebootFlag true
Done:
SectionEnd

Section "!${SSH_HANDLER}"
    SectionIn 1 RO
    SetOutPath -
    WriteUninstaller "${UNINST_EXE}"
    File "ssh-handler\bin\${SSH_HANDLER_CONFIGURATION}\${SWC_INPUT_TOOLKIT_DLL}"
    File "ssh-handler\bin\${SSH_HANDLER_CONFIGURATION}\${SWC_LAYOUT_TOOLKIT_DLL}"
    File "ssh-handler\bin\${SSH_HANDLER_CONFIGURATION}\${WPF_TOOLKIT_DLL}"
    File "ssh-handler\bin\${SSH_HANDLER_CONFIGURATION}\${SSH_HANDLER_EXE}"
    ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
    StrCmpS $MultiUser.InstallMode "CurrentUser" CurrentUser AllUsers
CurrentUser:
    !insertmacro REGISTRY HKCU
    Goto Done
AllUsers:
    !insertmacro REGISTRY HKLM
Done:
SectionEnd

Section "un.${SSH_HANDLER}"
    Delete /REBOOTOK "$INSTDIR\${SSH_HANDLER_EXE}"
    Delete /REBOOTOK "$INSTDIR\${SWC_INPUT_TOOLKIT_DLL}"
    Delete /REBOOTOK "$INSTDIR\${SWC_LAYOUT_TOOLKIT_DLL}"
    Delete /REBOOTOK "$INSTDIR\${WPF_TOOLKIT_DLL}"
    Delete /REBOOTOK "$INSTDIR\${UNINST_EXE}"
    RMDir /REBOOTOK $INSTDIR
    StrCmpS $MultiUser.InstallMode "CurrentUser" CurrentUser AllUsers
CurrentUser:
    !insertmacro UN_REGISTRY HKCU
    Goto Done
AllUsers:
    !insertmacro UN_REGISTRY HKLM
Done:
SectionEnd

Function .onInit
    !insertmacro MULTIUSER_INIT
    ClearErrors
    ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client" "Install"
    IfErrors EnableDotNetFramework
    IntCmp $0 1 Done
EnableDotNetFramework:
    ClearErrors
    SectionGetFlags ${dot_net_framework_index} $1
    IntOp $1 $1 | ${SF_SELECTED}
    SectionSetFlags ${dot_net_framework_index} $1
Done:
FunctionEnd

Function un.onInit
    !insertmacro MULTIUSER_UNINIT
FunctionEnd

# vim: autoindent
