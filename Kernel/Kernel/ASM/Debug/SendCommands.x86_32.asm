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