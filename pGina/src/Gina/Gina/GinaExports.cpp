/*
	Copyright (c) 2011, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors 
		  may be used to endorse or promote products derived from this software without 
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#include <windows.h>
#include <winwlx.h>

#include <Macros.h>

#include "Gina.h"
#include "Winlogon.h"
#include "Themes.h"

#define pGINA_FROM_CTX(context) pGina::GINA::Gina * pGina = static_cast<pGina::GINA::Gina *>(context)

/**
  Winlogon and a GINA DLL use this function to determine which version of the interface each
  	was written for.
 
  <strong>Thread Desktop:</strong> Winlogon Desktop\n
  <strong>Workstation:</strong> Locked\n
  
  @return TRUE - The GINA DLL can operate with this version of Winlogon.Continue initializing Winlogon. \n
   FALSE - The GINA DLL can not operate with this version of Winlogon.Winlogon (and the system) will not boot.
 */ 			 
BOOL WINAPI WlxNegotiate(DWORD dwWinlogonVersion, DWORD *pdwDllVersion) 
{	
	pDEBUG(L"WlxNegotiate(%d (0x%08x), ...)", dwWinlogonVersion, dwWinlogonVersion);
	// We support all versions from 1.3 up
	if(dwWinlogonVersion < WLX_VERSION_1_3)
		return FALSE;

	// We'll support what winlogon does
	*pdwDllVersion = dwWinlogonVersion;

	// Record winlogon version
	pGina::GINA::WinlogonInterface::Version(dwWinlogonVersion);
	return true;
}

/**
	Winlogon calls this function once for each window station present on the machine. (Note that
	 Windows NT 3.5 supports only one window station. This window station is called "Winsta0".
	 Additional physical window stations may be supported in futurereleases.) This allows the DLL to
	 initialize itself, including obtainingaddresses of Winlogon support functions used by this DLL.
	 The DLL can return a context pointer that will be passed in allfuture interactions from Winlogon to
	 GINA. This allows GINA to keep global context associated with this window station.

	We've added the calls necessary to ensure the theme service is started and working before we go any
     further, as explained in http://support.microsoft.com/default.aspx?scid=kb;en-us;322047
 
	@param lpWinsta Points to the name of the window station being initialized.
	@param hWlx Handle to Winlogon that must be provided in future calls related tothis window station.
	@param pwReserved Reserved
	@param pWinlogonFunctions Receives a pointer to a Winlogon function dispatch table. Thecontents of the table is dependent upon 
		the GINA DLL version returned from the WlxNegotiate() call. The table does not change, so the        
		GINA DLL can reference the table rather than copying it.                                             
	@param pWlxContext This is an OUT parameter. It allows GINA to return a 32­bit context value that will be provided in all
		future calls related to this window station. Generally the value returned will be something likea
		pointer to a context structure allocated by GINA for this window station.
	
	<strong>Thread Desktop:</strong> Winlogon Desktop\n
	<strong>Workstation:</strong> Locked\n
   	@return True - Indicates the Gina DLL successfully initialized.\n
			False - Indicates the Gina could not successfully initialize.\n
			<i>Note that If the DLL could not initialize, then the system will not boot.<i>
 */
BOOL WINAPI WlxInitialize(LPWSTR lpWinsta, HANDLE hWlx, PVOID pvReserved, PVOID pWinlogonFunctions, PVOID * pWlxContext) 
{
	pDEBUG(L"WlxInitialize(%s, 0x%08x, 0x%08x, 0x%08x, ...)", lpWinsta, hWlx, pvReserved, pWinlogonFunctions);
	pGina::GINA::Themes::Init();
	return (pGina::GINA::Gina::InitializeFactory(hWlx, pWinlogonFunctions, (pGina::GINA::Gina **) pWlxContext) ? TRUE : FALSE);
}


/**
	Winlogon calls this function when there is no one logged on.

	@param pWlxContext (IN parameter) Context value associated with this window station that GINA returned in the
		WlxInitialize() call.
		
	<strong>Thread Desktop:</strong> Winlogon Desktop\n
	<strong>Workstation:</strong> Locked\n
	
	@return None
 */
VOID WINAPI WlxDisplaySASNotice(PVOID pWlxContext) 
{
	pDEBUG(L"WlxDisplaySASNotice");
	pGINA_FROM_CTX(pWlxContext);
	pGina->DisplaySASNotice();
}

/**
	Winlogon calls this function when an SAS event is received and noone is logged on. This indicates
		that a logon attempt should be made.

	@param pWlxContext (IN parameter) Context value associated with this window station that GINA returned in the
		WlxInitialize() call.
	@param dwSasType (IN parameter) A value indicating what type of secure attentionsequence was entered. Values below
		WLX_SAS_TYPE_MAX_MSFT_VALUE are used to define Microsoft standard secure attention
		sequences. Values above this value are for definition by GINA developers.
	@param pAuthenticationId (IN parameter) The AuthenticationID associated with this logon.The GINA DLL should pass this
		value as the LogonId parameter to the LsaLogonUser() call. The pointer is good only until this
		routine returns. To keep the LUID value longer than that, the GINA DLL should copy it before
		returning.
	@param pLogonSid (IN parameter) This parameter contains a pointer to a SID. This sid is unique to this logon session.
		Winlogon uses this SID to change the protection on the window station and application desktop so
		that the newly logged on user can access them. To ensure proper user shell access to these objects,
		the GINA DLL should include this SID in theLocalGroups parameter passed to LsaLogonUser().
		Winlogon frees this SID upon return from thiscall, so if it is required by GINA after returning, a copy
		must be made in this routine.
	@param pdwOptions (OUT parameter) Receives a set of logon options. These options are defined by the manifest
		constants named WLX_LOGON_OPT_xxx.
	@param phToken (OUT parameter) Upon completion of a successful logon,receives a handle that must be filled in
		upon return if a logon was successfully performed. This handle value is typically receivedfrom
		LsaLogonUser(). Winlogon closes this handle when it isdone with it, so if the GINA DLL should be
		able to access the token, it should make a duplicate of this handle.
	@param pMprNotifyInfo (OUT parameter) This parameter contains a pointer to a structure for returning password information
		to other network providers. A GINA is not required to return this information. If a GINA returns
		password information, then it should fill in the pointers inthe structure. Any NULL field in the
		structure will be ignored byWinlogon. The strings should be allocated individually by theGINA, and
		they will be freed by Winlogon.
	@param pProfile (OUT parameter) Upon return from a successful authentication, this field must point to one of the
		WLX_PROFILE_xxx structures. The first DWORD in the profile structure is used to indicate which
		of the WLX_PROFILE_xxx structures is beingreturned. The information in this structure is used by
		Winlogon to load the logged on user's profile. This structure, and any strings orbuffers pointed to
		from withing this structure are freed byWinlogon when no longer needed.

	<strong>Thread Desktop:</strong> Winlogon Desktop\n
	<strong>Workstation:</strong> Locked\n

	@return WLX_SAS_ACTION_LOGON - A user has logged on.\n
			WLX_SAS_ACTION_NONE - A logon attempt was unsuccessful or cancelled.\n
			WLX_SAS_ACTION_SHUTDOWN - The user requested the system be shut down.\n
			WLX_SAS_ACTION_SHUTDOWN_REBOOT - The user requested the system be rebooted.\n
            WLX_SAS_ACTION_SHUTDOWN_POWER_OFF - The user requested that the system be turned off\n
*/
int WINAPI WlxLoggedOutSAS(PVOID pWlxContext, DWORD dwSasType, PLUID pAuthenticationId, PSID pLogonSid, 
						   PDWORD pdwOptions, PHANDLE phToken, PWLX_MPR_NOTIFY_INFO pMprNotifyInfo, PVOID *pProfile) 
{
	pDEBUG(L"WlxLoggedOutSAS");
	pGINA_FROM_CTX(pWlxContext);
	return pGina->LoggedOutSAS(dwSasType, pAuthenticationId, pLogonSid, pdwOptions, phToken, pMprNotifyInfo, pProfile);	
}

/**
	Winlogon calls this function following a successful logon. Itspurpose is to request GINA to activate
		the user shell program(s). Note that the user shell should be activated in this routine ratherthan in
		WlxLoggedOffSas() so that Winlogon has a chance to update its state, including setting workstation
		and desktop protections, before any logged­on user processes are allowed to run. The pszDesktop parameter 
		should be passed to the CreateProcess API through the field lpDesktop in the STARTUPINFO structure. This 
		field is designated "Reserved forfuture use. Must be NULL." in the Win32 documentation, but pass this parameter in.

	@param pWlxContext (IN parameter) Context value associated with this window station that GINA returned in the
		WlxInitialize() call.
	@param pszDesktopName (IN parameter) Name of the desktop on which to start the shell. This should be supplied to
		CreateProcess() in the lpStartupInfo­>lpDesktop field (q.v).
	@param pszMprLogonScripts (IN parameter) Script names returned from the provider DLLs. Provider DLLs may return scripts to
		be executed during logon. The GINA may reject these, but Winlogon will provide them ifthey are
		there.
	@param pEnvironment (IN parameter) Initial environment for the process. Winlogoncreates this environment and hands it
		off to the GINA. The GINA can modify this environment before using it to initialize the user'sshell.

	<strong>Thread Desktop:</strong> Application Desktop\n
	<strong>Workstation:</strong> Not Locked\n
	
	@return	TRUE - Indicates that the shell processes were started by the GINA DLL.\n
			FALSE - Indicates that the GINA could not start the shell, and that the logon session should be terminated by
					Winlogon.
	@Notes 
*/
BOOL WINAPI WlxActivateUserShell(PVOID pWlxContext, PWSTR pszDesktopName, PWSTR pszMprLogonScript, PVOID pEnvironment) 
{
	pDEBUG(L"WlxActivateUserShell");
	pGINA_FROM_CTX(pWlxContext);
	return (pGina->ActivateUserShell(pszDesktopName, pszMprLogonScript, pEnvironment) ? TRUE : FALSE);	
}

/**
	Winlogon calls this function when an SAS event is received, andthere is a user logged on. This
		indicates that the user needs to talk to the security system. Note, this is distinguished from when the
		workstation is locked; see below.
	
	This function is used generally when the logged on user wishes to shut down, log out, or lock the
		workstation. The extension DLL can lock the workstation by returning WLX_LOCK_WKSTA.
		Winlogon locks the workstation, and calls WlxWkstaLockedSAS the next time an SAS is received.
		The extension DLL can use the profile to determine any information needed about the system.

	@param pWlxContext (IN parameter) Context value associated with this window station that GINA returned in the
		WlxInitialize() call.
	@param dwSasType (IN parameter) A value indicating what type of secure attentionsequence was entered. Values below
		WLX_SAS_TYPE_MAX_MSFT_VALUE are used to define Microsoft standard secure attention
		sequences. Values above this value are for definition by GINA developers.
	@param pReserved (IN parameter) Reserved.

	<strong>Thread Desktop:</strong> Winlogon Desktop\n
	<strong>Workstation:</strong> Locked\n

	@return WLX_SAS_ACTION_NONE - Return to the default desktop.\n
			WLX_SAS_ACTION_LOCK_WKSTA - Lock the workstation, wait for next SAS.\n
            WLX_SAS_ACTION_LOGOFF - Log the user off of the workstation.\n
			WLX_SAS_ACTION_SHUTDOWN - Log the user off and shutdown the machine.\n
			WLX_SAS_ACTION_SHUTDOWN_REBOOT - Shut down and reboot the machine.\n
			WLX_SAS_ACTION_SHUTDOWN_POWER_OFF - Shut down and turn off the machine, if hardware allows.
			WLX_SAS_ACTION_PWD_CHANGED - Indicates that the user changed their password. Notify network providers.
			WLX_SAS_ACTION_TASKLIST - Invoke the task list.
*/
int WINAPI WlxLoggedOnSAS(PVOID pWlxContext, DWORD dwSasType, PVOID pReserved) 
{
	pDEBUG(L"WlxLoggedOnSAS");
	pGINA_FROM_CTX(pWlxContext);
	return pGina->LoggedOnSAS(dwSasType, pReserved);
}

/**
	Winlogon calls this function when the workstation is placed in thelocked state. This allows the GINA
		to display information aboutthe lock, such as who locked the workstation and when. The GINA
		should display a dialog box that will be interrupted by a WLX_WM_SAS message, much like the
		WlxDisplaySASNotice function. This function should display a notice that describes the machine as locked. 

	@param pWlxContext (IN parameter) Context value associated with this window station that GINA returned in the
		WlxInitialize() call.
	@return None
*/
VOID WINAPI WlxDisplayLockedNotice(PVOID pWlxContext) 
{
	pDEBUG(L"WlxDisplayLockedNotice");
	pGINA_FROM_CTX(pWlxContext);
	pGina->DisplayLockedNotice();
}

/**
	Winlogon calls this function before locking the workstation, if, forexample, a screen saver is marked
		as secure.

	@param pWlxContext IN parameter) Context value associated with this window station that GINA returned in the
		wlxInitialize() call.
	
	@return True - Indicates it is OK to lock the workstation.\n
			False - Indicates it is not OK to lock the workstation.\n
*/
BOOL WINAPI WlxIsLockOk(PVOID pWlxContext) 
{	
	pDEBUG(L"WlxIsLockOk");
	pGINA_FROM_CTX(pWlxContext);
	return (pGina->IsLockOk() ? TRUE : FALSE);
}

/**
	Winlogon calls this function when it receives an SAS and theworkstation is locked. GINA may return
		indicating the workstation is to remain locked, the workstation is to be unlocked, or the logged­on
		user is being forced to log off (whichleaves the workstation locked until the logoff is completed).
	
	@param pWlxContext (IN parameter) Context value associated with this window station that GINA returned in the
		WlxInitialize() call.
	@param dwSasType (IN parameter) A value indicating what type of secure attentionsequence was entered. Values below
		WLX_SAS_TYPE_MAX_MSFT_VALUE are used to define Microsoft standard secure attention
		sequences. Values above this value are for definition by GINA developers.

	<strong>Thread Desktop:</strong> Winlogon Desktop\n
	<strong>Workstation:</strong> Locked\n

	@return WLX_SAS_ACTION_NONE - Workstation remains locked.\n
			WLX_SAS_ACTION_UNLOCK_WKSTA - Unlock the workstation.\n
			WLX_SAS_ACTION_FORCE_LOGOFF - Force the user to log off.\n
*/
int WINAPI WlxWkstaLockedSAS(PVOID pWlxContext, DWORD dwSasType) 
{
	pDEBUG(L"WlxWkstaLockedSAS");
	pGINA_FROM_CTX(pWlxContext);
	return pGina->WkstaLockedSAS(dwSasType);
}

/**
	Winlogon calls this function when the user has initiated a logoff, foreaxmple by calling
		ExitWindowsEx(). The GINA can determine whether the logoff attempt is to be allowed.

	@param pWlxContext (IN parameter) Context value associated with this window station that GINA returned in the
		WlxInitialize() call.
	
	@return True - Indicates that it is OK to lock the workstation.\n
			False - Indicates that it is not OK to lock the workstation.\n
*/
BOOL WINAPI WlxIsLogoffOk(PVOID pWlxContext) 
{	
	pGINA_FROM_CTX(pWlxContext);
	return (pGina->IsLogoffOk() ? TRUE : FALSE);
}

/**
	Winlogon calls this function to notify GINA of a logoff on thisworkstation. No action is necessary.

	@param pWlxContext (IN parameter) Context value associated with this window station that GINA returned in the
		WlxInitialize() call.

	<strong>Thread Desktop:</strong> Winlogon Desktop\n
	<strong>Workstation:</strong> Locked\n
	
	@return None.
*/
VOID WINAPI WlxLogoff(PVOID pWlxContext) 
{
	pGINA_FROM_CTX(pWlxContext);
	pGina->Logoff();
}

/**
	Winlogon calls this function right before shutdown so GINA canperform any shutdown tasks, such as
		ejecting a smart card from a reader. The user has already logged off, and the WlxLogoff function has
		been called.

	@param pWlxContext (IN parameter) Context value associated with this window station that GINA returned in the
		WlxInitialize() call.
	@param ShutdownType (IN parameter) Type of shutdown, one of: WLX_SAS_ACTION_SHUTDOWN, WLX_SAS_ACTION_SHUTDOWN_REBOOT, or
		WLX_SAS_ACTION_SHUTDOWN_POWER_OFF.

	<strong>Thread Desktop:</strong> Application desktop if user logged on and the workstation isn't locked, otherwise Winlogon desktop.
	<strong>Workstation:</strong> Not locked if application desktop, locked if Winlogon desktop.
*/
VOID WINAPI WlxShutdown(PVOID pWlxContext, DWORD ShutdownType) 
{
	pGINA_FROM_CTX(pWlxContext);
	pGina->Shutdown(ShutdownType);

	// As noted by Keith Brown in his article, it turns out we can still get calls even after this is done,
	//	so we leak our Gina * class JIC.  We're on our way out anyway, so this isn't a concern in the long term.
}

/**
	The WlxScreenSaverNotify function may be implemented by a replacement GINA DLL. Winlogon calls this function immediately 
		before a screen saver is activated, allowing the GINA to interact with the screen saver program. Before calling WlxScreenSaverNotify, 
		Winlogon sets the desktop state so that the current desktop is the Winlogon desktop and sets the workstation state so that 
		the desktop is locked. 

	@param pWlxContext [in] Pointer to the GINA context associated with this window station. The GINA returns this context value when 
		Winlogon calls WlxInitialize for this station. 
	@param pSecure  [in, out] Pointer to a Boolean value that, on input, specifies whether the current screen saver is secure and, on 
		output, indicates whether the workstation should be locked. 

	@return If the screen saver should be activated, the function returns TRUE.\n
			If the screen saver should not be activated, the function returns FALSE.\n
*/
BOOL WINAPI WlxScreenSaverNotify(PVOID  pWlxContext, BOOL * pSecure) 
{
	pGINA_FROM_CTX(pWlxContext);
	return (pGina->ScreenSaverNotify(pSecure) ? TRUE: FALSE);
}

/**
	The WlxStartApplication function can be implemented by a replacement GINA DLL. Winlogon calls this function when the system needs an 
		application to be started in the context of the user. 

		There are two reasons that the system might need an application to start in the context of the user:\n
		   1. Windows Explorer has quit unexpectedly and needs to be restarted.\n
		   2. The extended task manager needs to run.\n

	The GINA can override this behavior, if appropriate, by using the WlxStartApplication function.
	
	Before calling WlxStartApplication, Winlogon sets the desktop state so that the current desktop is the Winlogon desktop and sets the 
	workstation state so that the desktop is locked. If the WlxStartApplication function is not exported by the GINA, Winlogon will 
	execute the process.                                                                                   
	
	@param pWlxContext  [in] Pointer to the GINA context associated with this window station. The GINA returns this context value when Winlogon 
			calls WlxInitialize for this station. 
	@param pszDesktopName [in] Specifies the name of the desktop on which to start the application. Pass this string to the CreateProcess or 
		CreateProcessAsUser functions through the lpDesktop member of the STARTUPINFO structure. 
	@param pEnvironment [in] Specifies the initial environment for the process. Winlogon creates this environment and hands it off to the GINA. 
		The GINA can modify this environment before using it to initialize the shell of the user. The GINA is responsible for calling the VirtualFree 
		function to free the memory allocated for pEnvironment. 
	@param pszCmdLine [in] Program to execute. 

	@return If the function successfully starts the application, the function returns TRUE.\n
			If the function fails or the application did not start, the function returns FALSE.
*/
BOOL WINAPI WlxStartApplication(PVOID pWlxContext, PWSTR pszDesktopName, PVOID pEnvironment, PWSTR pszCmdLine) 
{
	pGINA_FROM_CTX(pWlxContext);
	return (pGina->StartApplication(pszDesktopName, pEnvironment, pszCmdLine) ? TRUE : FALSE);
}

/**
	The WlxNetworkProviderLoad function must be implemented by a replacement GINA DLL. Winlogon calls this function to collect valid authentication 
		and identification information.

	The GINA is not required to return password information. Any NULL fields within the structure will be ignored by Winlogon. Use LocalAlloc 
		to allocate each string; Winlogon will free them when they are no longer needed. 

	@param pWlxContext  [in] Pointer to the GINA context associated with this window station. The GINA returns this context value when Winlogon calls 
			WlxInitialize for this station. 
	@param pNprNotifyInfo  [out] Points to an WLX_MPR_NOTIFY_INFO structure that contains domain, user name, and password information for the user. 
			Winlogon will use this information to provide identification and authentication information to network providers. 

	@return TRUE - Return TRUE if the user was authenticated. \n
			FALSE Return FALSE if the user was not authenticated.\n 
*/
BOOL WINAPI WlxNetworkProviderLoad(PVOID pWlxContext, PWLX_MPR_NOTIFY_INFO pNprNotifyInfo) 
{
	pGINA_FROM_CTX(pWlxContext);
	return (pGina->NetworkProviderLoad(pNprNotifyInfo) ? TRUE : FALSE);
}

/**
	The WlxDisplayStatusMessage function must be implemented by a replacement GINA DLL. Winlogon calls this function when the GINA DLL 
		should display a message.

	@param pWlxContext [in] Pointer to the GINA context associated with this window station. The GINA returns this context value when 
		Winlogon calls WlxInitialize for this station. 
	@param hDesktop [in] Handle to the desktop where the status message should be displayed. 
	@param dwOptions [in] Specifies display options for the status dialog box. The following options are valid: \n
		STATUSMSG_OPTION_NOANIMATION\n
		STATUSMSG_OPTION_SETFOREGROUND\n
	@param pTitle  [in] Pointer to a null-terminated wide character string that specifies the title of the message to be displayed. 
	@param pMessage [in] Pointer to a null-terminated wide character string that specifies the message to be displayed. 
	
	@return TRUE - Returns TRUE if the message was displayed. \n
			FALSE - Returns FALSE if the message was not displayed. \n
*/
BOOL WINAPI WlxDisplayStatusMessage(PVOID pWlxContext, HDESK hDesktop, DWORD dwOptions, PWSTR pTitle, PWSTR pMessage) 
{
	pGINA_FROM_CTX(pWlxContext);
	return (pGina->DisplayStatusMessage(hDesktop, dwOptions, pTitle, pMessage) ? TRUE : FALSE);
}

/**
	The WlxGetStatusMessage function must be implemented by a replacement GINA DLL. Winlogon calls this function to get the status 
		message being displayed by the GINA DLL.

	@param pWlxContext [in] Pointer to the GINA context associated with this window station. The GINA returns this context value 
		when Winlogon calls WlxInitialize for this station. 
	@param pdwOptions [out] Pointer to a DWORD that will hold the display options for the current status message. 
	@param pMessage [out] Returns the current status message text. 
	@param dwBufferSize [in] Size of the pMessage buffer. 

	@return TRUE - Returns TRUE if the message was retrieved. \n
			FALSE - Returns FALSE if the message was not retrieved. \n
*/
BOOL WINAPI WlxGetStatusMessage(PVOID   pWlxContext, DWORD * pdwOptions, PWSTR   pMessage, DWORD   dwBufferSize) 
{
	pGINA_FROM_CTX(pWlxContext);
	return (pGina->GetStatusMessage(pdwOptions, pMessage, dwBufferSize) ? TRUE : FALSE);
}

/**
	The WlxRemoveStatusMessage function must be implemented by a replacement GINA DLL. Winlogon calls this function to tell 
		the GINA DLL to stop displaying the status message.

	@param pWlxContext [in] Pointer to the GINA context associated with this window station. The GINA returns this context 
		value when Winlogon calls WlxInitialize for this station. 

	@param TRUE - Return TRUE if the message was removed.\n
			FALSE Return FALSE if the message was not removed.\n
*/
BOOL WINAPI WlxRemoveStatusMessage(PVOID pWlxContext) 
{
	pGINA_FROM_CTX(pWlxContext);
	return (pGina->RemoveStatusMessage() ? TRUE : FALSE);
}

/**
	TS session is being reconnected
*/
VOID WINAPI WlxReconnectNotify(PVOID pWlxContext) 
{
	pGINA_FROM_CTX(pWlxContext);
	pGina->ReconnectNotify();
}

/**
	TS session disconnect
*/
VOID WINAPI WlxDisconnectNotify(PVOID pWlxContext) 
{	
	pGINA_FROM_CTX(pWlxContext);
	pGina->DisconnectNotify();
}

/**
	The WlxGetConsoleSwitchCredentials function must be implemented by a replacement GINA DLL. Winlogon calls this function to 
		read the currently logged on user's credentials to transparently transfer them to a target session.

	@param pWlxContext  [in] Pointer to a GINA-specific context. 
	@param pInfo  [out] Pointer to a WLX_CONSOLESWITCH_CREDENTIALS_INFO_V1_0 to return GINA relevant information. 
	
	@return Returns TRUE on success and FALSE on failure.
*/
BOOL WINAPI WlxGetConsoleSwitchCredentials(PVOID  pWlxContext, PVOID  pCredInfo) 
{
	pGINA_FROM_CTX(pWlxContext);	
	return (pGina->GetConsoleSwitchCredentials(pCredInfo) ? TRUE : FALSE);
}

VOID WINAPI zDebugEntryPoint()
{
	DWORD fakeDllVersion = 0;
	WlxNegotiate(WLX_VERSION_1_4, &fakeDllVersion);    

	void * context = 0;
    WlxInitialize(0, 0, 0, 0, &context);    
    WlxLoggedOutSAS(context, 1, NULL, NULL, 0, NULL, NULL, NULL);
}