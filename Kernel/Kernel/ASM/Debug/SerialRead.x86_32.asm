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


; Method not written to C# calling convention. Uses debugger internel calling convention - optimised system.
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