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

GLOBAL method_System_Byte__RETEND_Kernel_Kernel_DECLEND_GetKernelStackPtr_NAMEEND___:function
GLOBAL method_System_UInt32_RETEND_Kernel_Kernel_DECLEND_GetESP_NAMEEND___:function
GLOBAL method_System_UInt32_RETEND_Kernel_Kernel_DECLEND_GetStackValue_NAMEEND__System_UInt32_:function

EXTERN Kernel_Stack


method_System_Byte__RETEND_Kernel_Kernel_DECLEND_GetKernelStackPtr_NAMEEND___:
push dword ebp
mov dword ebp, esp

mov dword [ebp+8], Kernel_Stack

pop dword ebp
ret



method_System_UInt32_RETEND_Kernel_Kernel_DECLEND_GetESP_NAMEEND___:
push dword ebp
mov dword ebp, esp

mov ebx, esp
add ebx, 12
mov dword [ebp+8], ebx

pop dword ebp
ret

method_System_UInt32_RETEND_Kernel_Kernel_DECLEND_GetStackValue_NAMEEND__System_UInt32_:
push dword ebp
mov dword ebp, esp

mov dword eax, [ebp+12]
mov dword ebx, esp
add ebx, 16
add ebx, eax
mov ebx, [ebx]
mov dword [ebp+8], ebx

pop dword ebp
ret