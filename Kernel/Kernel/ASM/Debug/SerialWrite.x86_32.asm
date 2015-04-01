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

GLOBAL method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt32_System_UInt32_:function
GLOBAL method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteUInt16_System_UInt16_:function
GLOBAL method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteByte_System_Byte_:function
GLOBAL method_System_Void_Kernel_Debug_BasicDebug_Serial_WriteString_System_String_:function
GLOBAL BasicDebug_SerialWrite32:function
GLOBAL BasicDebug_SerialWrite16:function
GLOBAL BasicDebug_SerialWrite8:function

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