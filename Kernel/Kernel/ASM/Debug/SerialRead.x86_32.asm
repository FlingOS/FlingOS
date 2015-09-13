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

GLOBAL BasicDebug_SerialRead8:function
GLOBAL BasicDebug_SerialRead16:function
GLOBAL BasicDebug_SerialRead32:function
GLOBAL method_System_Byte_Kernel_Debug_BasicDebug_Serial_ReadByte__:function
GLOBAL method_System_UInt16_Kernel_Debug_BasicDebug_Serial_ReadUInt16__:function
GLOBAL method_System_UInt32_Kernel_Debug_BasicDebug_Serial_ReadUInt32__:function
GLOBAL method_System_UInt32_RETEND_Kernel_Debug_BasicDebug_DECLEND_Serial_SafeReadUInt32_NAMEEND___:function

EXTERN BasicDebug_ReadAttempts
EXTERN BasicDebug_Serial_ComPortMemAddr

; BEGIN - BasicDebug : Serial Read

BasicDebug_SerialRead8:
; Load read attempts
mov dword ebx, [BasicDebug_ReadAttempts]

; Load the port address
mov dx, [BasicDebug_Serial_ComPortMemAddr]
; Move to the "data available" bit
add dx, 5
BasicDebug_SerialRead8_Wait:
; Read the "data available" bit
in al, dx
; Test whether it is zero
test al, 0x01
; If it isn't, don't continue waiting.
jnz BasicDebug_SerialRead8_ReturnVal
; Only continue waiting if ebx == 0 || ebx > 1 i.e. ebx != 1
;	else return 0
cmp ebx, 1
je BasicDebug_SerialRead8_Return0
; If ebx == 0, continue waiting
;   else decrement ebx
cmp ebx, 0
jz BasicDebug_SerialRead8_Wait
dec ebx
jmp BasicDebug_SerialRead8_Wait

BasicDebug_SerialRead8_ReturnVal:
; Reload the port address
mov dx, [BasicDebug_Serial_ComPortMemAddr]
; Read the data
in al, dx
; Return...
ret
BasicDebug_SerialRead8_Return0:
mov al, 0
ret

BasicDebug_SerialRead16:
; Read the bytes
call BasicDebug_SerialRead8
ror eax, 8
call BasicDebug_SerialRead8
ror eax, 8
; Return
ret

BasicDebug_SerialRead32:
; Read the bytes
call BasicDebug_SerialRead8
ror eax, 8
call BasicDebug_SerialRead8
ror eax, 8
call BasicDebug_SerialRead8
ror eax, 8
call BasicDebug_SerialRead8
ror eax, 8
; Return
ret


; Method not written to C# calling convention. Uses debugger internal calling convention - optimised system.
method_System_Byte_Kernel_Debug_BasicDebug_Serial_ReadByte__:

push ebp
mov ebp, esp

; Clear the return value
mov eax, 0
; Read the value
call BasicDebug_SerialRead8

pop ebp

; Return
ret


; Method not written to C# calling convention. Uses debugger internel calling convention - optimised system.
method_System_UInt16_Kernel_Debug_BasicDebug_Serial_ReadUInt16__:

push ebp
mov ebp, esp

; Clear the return value
mov eax, 0
; Read the value
call BasicDebug_SerialRead16

pop ebp

; Return
ret

; Method not written to C# calling convention. Uses debugger internal calling convention - optimized system.
method_System_UInt32_Kernel_Debug_BasicDebug_Serial_ReadUInt32__:

push ebp
mov ebp, esp

; Clear the return value
mov eax, 0
; Read the value
call BasicDebug_SerialRead32

pop ebp

; Return
ret


; Method written to C# calling convention
method_System_UInt32_RETEND_Kernel_Debug_BasicDebug_DECLEND_Serial_SafeReadUInt32_NAMEEND___:

push ebp
mov ebp, esp

; Clear the return value
mov eax, 0
; Read the value
call BasicDebug_SerialRead32

mov [ebp+8], eax

pop ebp

; Return
ret

; END - Basic Debug : Serial Read