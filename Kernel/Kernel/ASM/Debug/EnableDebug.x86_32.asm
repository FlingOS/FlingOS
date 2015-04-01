; - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  ;
;                                                                                ;
;               All contents copyright Edward Nutting 2014                       ;
;                                                                                ;
;        You may not share, reuse, redistribute or otherwise use the             ;
;        contents this file outside of the Fling OS project without              ;
;        the express permission of Edward Nutting or other copyright             ;
;        holder. Any changes (including but not limited to additions,            ;
;        edits or subtractions) made to or from this document are not            ;
;        your copyright. They are the copyright of the main copyright            ;
;        holder for all Fling OS files. At the time of writing, this             ;
;        owner was Edward Nutting. To be clear, owner(s) do not include          ;
;        developers, contributors or other project members.                      ;
;                                                                                ;
; - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  ;

BITS 32

SECTION .text

GLOBAL method_System_Void_RETEND_Kernel_Debug_BasicDebug_DECLEND_BeginEnableDebug_NAMEEND___:function
GLOBAL method_System_Void_RETEND_Kernel_Debug_BasicDebug_DECLEND_EndEnableDebug_NAMEEND___:function

EXTERN BasicDebug_Enabled
EXTERN BasicDebug_SendConnectedCmd
EXTERN BasicDebug_ReadAttempts

method_System_Void_RETEND_Kernel_Debug_BasicDebug_DECLEND_BeginEnableDebug_NAMEEND___:
mov dword [BasicDebug_Enabled], 1
call BasicDebug_SendConnectedCmd
ret

method_System_Void_RETEND_Kernel_Debug_BasicDebug_DECLEND_EndEnableDebug_NAMEEND___:
mov dword [BasicDebug_ReadAttempts], 0
ret