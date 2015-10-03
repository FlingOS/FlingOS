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

GLOBAL method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_:function

method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_:

; Load distance
mov eax, [esp+4]

; Load current (i.e. start) pointer
mov ebx, [esp+8]

.Loop:

; Load value to copy
mov ecx, [ebx]

; Load / calc pointer to copy to
mov edx, ebx
add edx, eax
mov [edx], ecx

; Shift to next dword
sub ebx, 4

; Is current pointer == end pointer
;		i.e. Is ebx == esp
cmp ebx, esp
jne .Loop

ret