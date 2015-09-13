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

GLOBAL BasicDebug_SendConnectedCmd:function
GLOBAL BasicDebug_SendBreakCmd:function
GLOBAL BasicDebug_SendBreakAddress:function
GLOBAL BasicDebug_SendRegisters:function
GLOBAL BasicDebug_SendArguments:function
GLOBAL BasicDebug_SendLocals:function
GLOBAL BasicDebug_SendMemory:function
GLOBAL BasicDebug_SendCmd:function

EXTERN BasicDebug_ConnectedCmd
EXTERN BasicDebug_BreakCmd
EXTERN BasicDebug_SendBreakAddressCmd
EXTERN BasicDebug_SendRegistersCmd
EXTERN BasicDebug_SendArgumentsCmd
EXTERN BasicDebug_SendLocalsCmd
EXTERN BasicDebug_SendMemoryCmd

EXTERN BasicDebug_CallerEIP
EXTERN BasicDebug_CallerESP
EXTERN BasicDebug_CallerEBP
EXTERN BasicDebug_RegistersESP

EXTERN method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteByte_System_Byte_
EXTERN method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
EXTERN method_System_UInt32_Kernel_Debug_BasicDebug_Serial_ReadUInt32__

; BEGIN - Basic Debug : Send Commands

BasicDebug_SendConnectedCmd:
push eax

mov al, [BasicDebug_ConnectedCmd]
call BasicDebug_SendCmd

pop eax
ret



BasicDebug_SendBreakCmd:
push eax

mov al, [BasicDebug_BreakCmd]
call BasicDebug_SendCmd

pop eax
ret




BasicDebug_SendBreakAddress:
push eax

mov al, [BasicDebug_SendBreakAddressCmd]
call BasicDebug_SendCmd

push dword [BasicDebug_CallerEIP]
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
add esp, 4

pop eax
ret




BasicDebug_SendRegisters:
push eax

mov al, [BasicDebug_SendRegistersCmd]
call BasicDebug_SendCmd

mov eax, [BasicDebug_RegistersESP]
push dword [eax+0]
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
add esp, 4
 
mov eax, [BasicDebug_RegistersESP]
push dword [eax+4]
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
add esp, 4
 
mov eax, [BasicDebug_RegistersESP]
push dword [eax+8]
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
add esp, 4
  
mov eax, [BasicDebug_RegistersESP]
push dword [eax+12]
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
add esp, 4
  
mov eax, [BasicDebug_RegistersESP]
push dword [eax+16]
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
add esp, 4
  
mov eax, [BasicDebug_RegistersESP]
push dword [eax+20]
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
add esp, 4
  
mov eax, [BasicDebug_RegistersESP]
push dword [eax+24]
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
add esp, 4
  
mov eax, [BasicDebug_RegistersESP]
push dword [eax+28]
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
add esp, 4
 

; Send ESP manually
push dword [BasicDebug_CallerESP]
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
add esp, 4


pop eax
ret




BasicDebug_SendArguments:
push eax

; Get the number of bytes in the stack to go
; Return value stored in eax
call method_System_UInt32_Kernel_Debug_BasicDebug_Serial_ReadUInt32__
push eax

mov al, [BasicDebug_SendArgumentsCmd]
call BasicDebug_SendCmd

pop eax

cmp eax, 0
jz BasicDebug_SendArguments_End

mov ebx, [BasicDebug_CallerEBP]
; Skip over calling convention stuff
add ebx, 8
mov ecx, eax

BasicDebug_SendArguments_Loop:
mov byte al, [ebx]
push dword eax
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteByte_System_Byte_
add esp, 4
add ebx, 1
loop BasicDebug_SendArguments_Loop

BasicDebug_SendArguments_End:

pop eax
ret




BasicDebug_SendLocals:
push eax

mov al, [BasicDebug_SendLocalsCmd]
call BasicDebug_SendCmd

mov ebx, [BasicDebug_CallerESP]
mov edx, [BasicDebug_CallerEBP]
mov ecx, edx
sub ecx, ebx

push dword ecx
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_
add esp, 4

cmp ecx, 0
jz BasicDebug_SendLocals_End

BasicDebug_SendLocals_Loop:
mov byte al, [ebx]
push dword eax
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteByte_System_Byte_
add esp, 4
add ebx, 1
loop BasicDebug_SendLocals_Loop


BasicDebug_SendLocals_End:

pop eax
ret



BasicDebug_SendMemory:
push eax

mov al, [BasicDebug_SendMemoryCmd]
call BasicDebug_SendCmd

; Get starting address
call method_System_UInt32_Kernel_Debug_BasicDebug_Serial_ReadUInt32__
mov ebx, eax

; Get num bytes to send
push ebx
call method_System_UInt32_Kernel_Debug_BasicDebug_Serial_ReadUInt32__
mov ecx, eax
pop ebx

BasicDebug_SendMemory_Loop:
mov byte al, [ebx]

push dword ebx
push dword ecx

push dword eax
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteByte_System_Byte_
add esp, 4

pop dword ecx
pop dword ebx

add ebx, 1
loop BasicDebug_SendMemory_Loop

pop eax
ret



BasicDebug_SendCmd:
push dword eax
call method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteByte_System_Byte_
add esp, 4
ret

; END - Basic Debug : Send Commands