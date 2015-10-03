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

GLOBAL method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_:function
GLOBAL method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt16_System_UInt16_:function
GLOBAL method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteByte_System_Byte_:function
GLOBAL method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteString_System_String_:function
GLOBAL BasicDebug_SerialWrite32:function
GLOBAL BasicDebug_SerialWrite16:function
GLOBAL BasicDebug_SerialWrite8:function

EXTERN BasicDebug_Serial_ComPortMemAddr
EXTERN BasicDebug_Enabled

; BEGIN - Basic Debug : Serial Write

method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_:

push dword ebp
mov dword ebp, esp

pushad

mov eax, [BasicDebug_Enabled]
cmp eax, 0
jz .End

; Load the int to write
mov dword eax, [ebp+8]

; Call the write function
push dword eax
mov dword esi, esp
call BasicDebug_SerialWrite32
pop dword eax

.End:

popad

pop dword ebp

ret

method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt16_System_UInt16_:

push dword ebp
mov dword ebp, esp

pushad

mov eax, [BasicDebug_Enabled]
cmp eax, 0
jz .End

; Load the int to write
mov eax, 0
mov word ax, [ebp+8]

; Call the write function
push dword eax
mov dword esi, esp
call BasicDebug_SerialWrite16
pop dword eax

.End:

popad

pop dword ebp

ret

method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteByte_System_Byte_:

push dword ebp
mov dword ebp, esp

pushad

mov eax, [BasicDebug_Enabled]
cmp eax, 0
jz .End

; Load the int to write
mov eax, 0
mov byte al, [ebp+8]

; Call the write function
push dword eax
mov dword esi, esp
call BasicDebug_SerialWrite8
pop dword eax

.End:

popad

pop dword ebp

ret

method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteString_System_String_:

push dword ebp
mov dword ebp, esp

mov eax, [ebp+8]
mov dword ecx, [eax]
mov dword esi, eax

; Write the length
call BasicDebug_SerialWrite8
call BasicDebug_SerialWrite8
call BasicDebug_SerialWrite8
call BasicDebug_SerialWrite8

; Write the string
.loop1:
call BasicDebug_SerialWrite8
add esi, 3
loop .loop1

pop dword ebp

ret

BasicDebug_SerialWrite32:
call BasicDebug_SerialWrite8
call BasicDebug_SerialWrite8
call BasicDebug_SerialWrite8
call BasicDebug_SerialWrite8
ret

BasicDebug_SerialWrite16:
call BasicDebug_SerialWrite8
call BasicDebug_SerialWrite8
ret

BasicDebug_SerialWrite8:

mov dx, [BasicDebug_Serial_ComPortMemAddr]
add dx, 5

BasicDebug_SerialWrite8_Wait:
in al, dx
test al, 0x20
jz BasicDebug_SerialWrite8_Wait

mov dx, [BasicDebug_Serial_ComPortMemAddr]
mov al, [esi]
out dx, al

inc esi
ret

; END - Basic Debug : Serial Write