; - - - - - - - - - - - - - - - - - - - LICENSE - - - - - - - - - - - - - - - -  ;
;
;    Fling OS - The educational operating system
;    Copyright (C) 2015 Edward Nutting
;
;    This program is free software: you can redistribute it and/or modify
;    it under the terms of the GNU General Public License as published by
;    the Free Software Foundation, either version 2 of the License, or
;    (at your option) any later version.
;
;    This program is distributed in the hope that it will be useful,
;    but WITHOUT ANY WARRANTY; without even the implied warranty of
;    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
;    GNU General Public License for more details.
;
;    You should have received a copy of the GNU General Public License
;    along with this program.  If not, see <http:;www.gnu.org/licenses/>.
;
;  Project owner: 
;		Email: edwardnutting@outlook.com
;		For paper mail address, please contact via email for details.
;
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