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

GLOBAL method_System_Byte__RETEND_FlingOops_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___:function
GLOBAL method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__:function

method_System_Byte__RETEND_FlingOops_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___:
push dword ebp
mov dword ebp, esp

mov dword eax, esp
add eax, 12
mov dword [ebp+8], eax

pop dword ebp
ret


method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__:
mov dword eax, [esp]
mov dword esp, [esp+4]
; Handles the "add esp, 4" after return
sub esp, 4
push dword eax
ret