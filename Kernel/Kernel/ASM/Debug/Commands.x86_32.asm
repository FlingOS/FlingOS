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