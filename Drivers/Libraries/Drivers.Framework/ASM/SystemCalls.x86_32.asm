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

GLOBAL method_System_Void_RETEND_Drivers_Framework_Processes_SystemCalls_DECLEND_Call_NAMEEND__Drivers_Framework_Processes_SystemCallNumbers_System_UInt32_System_UInt32_System_UInt32_System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__:function

SECTION .text
method_System_Void_RETEND_Drivers_Framework_Processes_SystemCalls_DECLEND_Call_NAMEEND__Drivers_Framework_Processes_SystemCallNumbers_System_UInt32_System_UInt32_System_UInt32_System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__:
push dword ebp
mov dword ebp, esp

; Load arguments into argument registers
mov dword eax, [ebp+36]
mov dword ebx, [ebp+32]
mov dword ecx, [ebp+28]
mov dword edx, [ebp+24]
; Do the system call
int 48
; Push return values
push eax
push ebx
push ecx
push edx
; Store return values
;	Note: Arguments are references (pointers) to integers
;		  So the value of the arguments are addresses
mov dword eax, [ebp+8]
pop dword [eax]
mov dword eax, [ebp+12]
pop dword [eax]
mov dword eax, [ebp+16]
pop dword [eax]
mov dword eax, [ebp+20]
pop dword [eax]

pop dword ebp
ret