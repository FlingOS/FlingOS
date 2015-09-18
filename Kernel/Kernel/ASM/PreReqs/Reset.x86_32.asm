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
GLOBAL File_Reset:function
File_Reset:

GLOBAL method_System_Void_Kernel_PreReqs_Reset__:function
GLOBAL method_System_Void_RETEND_Kernel_PreReqs_DECLEND_Reset_NAMEEND___:function

; BEGIN - Reset
method_System_Void_Kernel_PreReqs_Reset__:
method_System_Void_RETEND_Kernel_PreReqs_DECLEND_Reset_NAMEEND___:
	cli ; Clear all interrupts so we aren't re-awoken
	hlt	; Halt the OS / CPU / etc.
	jmp method_System_Void_Kernel_PreReqs_Reset__ ; Just in case...
; END - Reset