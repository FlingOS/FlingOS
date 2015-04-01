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

BITS 32

SECTION .text

GLOBAL BasicDebug_Execute:function
GLOBAL BasicDebug_SetInt1TrapFlag:function
GLOBAL BasicDebug_ClearInt1TrapFlag:function
GLOBAL BasicDebug_SetInt3:function
GLOBAL BasicDebug_ClearInt3:function

EXTERN BasicDebug_SendBreakCmd
EXTERN BasicDebug_WaitForCommand
EXTERN BasicDebug_ContinueCmd
EXTERN BasicDebug_StepNextCmd
EXTERN BasicDebug_GetBreakAddressCmd
EXTERN BasicDebug_GetRegistersCmd
EXTERN BasicDebug_GetArgumentsCmd
EXTERN BasicDebug_GetLocalsCmd
EXTERN BasicDebug_SetInt3Cmd
EXTERN BasicDebug_ClearInt3Cmd
EXTERN BasicDebug_GetMemoryCmd

; BEGIN - BasicDebug_Execute
; See BasicDebug_InterruptHandler for global variables

BasicDebug_Execute:

push dword ebp
mov dword ebp, esp

call BasicDebug_SendBreakCmd

BasicDebug_ExecuteLoop:

call BasicDebug_WaitForCommand
; Command received stored in AL

; Continue command
cmp al, [BasicDebug_ContinueCmd]
jne BasicDebug_Execute_Skip1
jmp BasicDebug_ExecuteLoop_Leave
BasicDebug_Execute_Skip1:
 
; Step Next command
cmp al, [BasicDebug_StepNextCmd]
jne BasicDebug_Execute_Skip2
call BasicDebug_SetInt1TrapFlag
jmp BasicDebug_ExecuteLoop_Leave
BasicDebug_Execute_Skip2:
 
; Get Break Address command
cmp al, [BasicDebug_GetBreakAddressCmd]
jne BasicDebug_Execute_Skip3
call BasicDebug_SendBreakAddress
jmp BasicDebug_ExecuteLoop_Continue
BasicDebug_Execute_Skip3:
 
; Get Registers command
cmp al, [BasicDebug_GetRegistersCmd]
jne BasicDebug_Execute_Skip4
call BasicDebug_SendRegisters
jmp BasicDebug_ExecuteLoop_Continue
BasicDebug_Execute_Skip4:

; Get Arguments command
cmp al, [BasicDebug_GetArgumentsCmd]
jne BasicDebug_Execute_Skip5
call BasicDebug_SendArguments
jmp BasicDebug_ExecuteLoop_Continue
BasicDebug_Execute_Skip5:

; Get Locals command
cmp al, [BasicDebug_GetLocalsCmd]
jne BasicDebug_Execute_Skip6
call BasicDebug_SendLocals
jmp BasicDebug_ExecuteLoop_Continue
BasicDebug_Execute_Skip6:

; Set Int3 command
cmp al, [BasicDebug_SetInt3Cmd]
jne BasicDebug_Execute_Skip7
call BasicDebug_SetInt3
jmp BasicDebug_ExecuteLoop_Continue
BasicDebug_Execute_Skip7:

; Clear Int3 command
cmp al, [BasicDebug_ClearInt3Cmd]
jne BasicDebug_Execute_Skip8
call BasicDebug_ClearInt3
jmp BasicDebug_ExecuteLoop_Continue
BasicDebug_Execute_Skip8:

; Get Memory command
cmp al, [BasicDebug_GetMemoryCmd]
jne BasicDebug_Execute_Skip9
call BasicDebug_SendMemory
jmp BasicDebug_ExecuteLoop_Continue
BasicDebug_Execute_Skip9:

BasicDebug_ExecuteLoop_Continue:

mov eax, 0
jmp BasicDebug_ExecuteLoop
BasicDebug_ExecuteLoop_Leave:

pop dword ebp

ret


BasicDebug_SetInt1TrapFlag:
push ebp
push eax

; Load the stack to the correct location for accessing EFLAGS
; This is lower down the stack than EFLAGS
mov ebp, [BasicDebug_CallerESP]

; Set the Trap Flag (http://en.wikipedia.org/wiki/Trap_flag)
; For EFLAGS we want - the interrupt frame = ESP - 12
;					 - The interrupt frame + 8 for correct byte = ESP - 12 + 8 = ESP - 4
;					 - Therefore, ESP - 4 to get to the correct position
sub ebp, 4
mov eax, [ebp]
or eax, 0x0100
mov [ebp], eax

pop eax
pop ebp
ret


BasicDebug_ClearInt1TrapFlag:
push ebp
push eax

; See SetInt1TrapFlag
mov ebp, [BasicDebug_CallerESP]

sub ebp, 4
mov eax, [ebp]
and eax, 0xFEFF
mov [ebp], eax

pop eax
pop ebp
ret



BasicDebug_SetInt3:
push ebp
push eax

; Get the address to set Int3 at
call method_System_UInt32_Kernel_Debug_BasicDebug_Serial_ReadUInt32__
; Set the Int3
mov byte [eax], 0xCC

pop eax
pop ebp
ret


BasicDebug_ClearInt3:
push ebp
push eax

; Get the address to clear Int3 at
call method_System_UInt32_Kernel_Debug_BasicDebug_Serial_ReadUInt32__
; Clear the Int3 to 1-byte Nop
mov byte [eax], 0x90

pop eax
pop ebp
ret


; END - BasicDebug_Execute