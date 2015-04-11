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

GLOBAL BasicDebug_BreakCmd:data
GLOBAL BasicDebug_ContinueCmd:data
GLOBAL BasicDebug_StepNextCmd:data
GLOBAL BasicDebug_GetBreakAddressCmd:data
GLOBAL BasicDebug_SendBreakAddressCmd:data
GLOBAL BasicDebug_GetRegistersCmd:data
GLOBAL BasicDebug_SendRegistersCmd:data
GLOBAL BasicDebug_GetArgumentsCmd:data
GLOBAL BasicDebug_SendArgumentsCmd:data
GLOBAL BasicDebug_GetLocalsCmd:data
GLOBAL BasicDebug_SendLocalsCmd:data
GLOBAL BasicDebug_MessageCmd:data
GLOBAL BasicDebug_SetInt3Cmd:data
GLOBAL BasicDebug_ClearInt3Cmd:data
GLOBAL BasicDebug_GetMemoryCmd:data
GLOBAL BasicDebug_SendMemoryCmd:data
GLOBAL BasicDebug_ConnectedCmd:data

BasicDebug_BreakCmd db 1
BasicDebug_ContinueCmd db 2
BasicDebug_StepNextCmd db 3
BasicDebug_GetBreakAddressCmd db 4
BasicDebug_SendBreakAddressCmd db 5
BasicDebug_GetRegistersCmd db 6
BasicDebug_SendRegistersCmd db 7
BasicDebug_GetArgumentsCmd db 8
BasicDebug_SendArgumentsCmd db 9
BasicDebug_GetLocalsCmd db 10
BasicDebug_SendLocalsCmd db 11
BasicDebug_MessageCmd db 12
BasicDebug_SetInt3Cmd db 13
BasicDebug_ClearInt3Cmd db 14
BasicDebug_GetMemoryCmd db 15
BasicDebug_SendMemoryCmd db 16
BasicDebug_ConnectedCmd db 17