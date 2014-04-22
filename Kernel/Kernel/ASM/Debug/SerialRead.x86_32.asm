; BEGIN - BasicDebug : Serial Read

BasicDebug_SerialRead8:
; Load the port address
mov dx, [BasicDebug_Serial_ComPortMemAddr]
; Moev to the "data available" bit
add dx, 5
BasicDebug_SerialRead8_Wait:
; Read the "data available" bit
in al, dx
; Test whether it is zero
test al, 0x01
; If it is, continue waiting.
jz BasicDebug_SerialRead8_Wait

; Reload the port address
mov dx, [BasicDebug_Serial_ComPortMemAddr]
; Read the data
in al, dx
; Return...
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

; END - Basic Debug : Serial Read