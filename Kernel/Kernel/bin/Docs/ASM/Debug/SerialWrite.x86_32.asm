; BEGIN - Basic Debug : Serial Write

method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_:

push dword ebp
mov dword ebp, esp

; Load the int to write
mov dword eax, [ebp+8]

; Call the write function
push dword eax
mov dword esi, esp
call BasicDebug_SerialWrite32
pop dword eax

pop dword ebp

ret

method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt16_System_UInt16_:

push dword ebp
mov dword ebp, esp

; Load the int to write
mov eax, 0
mov word ax, [ebp+8]

; Call the write function
push dword eax
mov dword esi, esp
call BasicDebug_SerialWrite16
pop dword eax

pop dword ebp

ret

method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteByte_System_Byte_:

push dword ebp
mov dword ebp, esp

; Load the int to write
mov eax, 0
mov byte al, [ebp+8]

; Call the write function
push dword eax
mov dword esi, esp
call BasicDebug_SerialWrite8
pop dword eax

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
method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteString_System_String_.loop1:
call BasicDebug_SerialWrite8
add esi, 3
loop method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteString_System_String_.loop1

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