BITS 32

SECTION .text

GLOBAL method_System_Void_RETEND_Drivers_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_:function

method_System_Void_RETEND_Drivers_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_:

; Load distance
mov eax, [esp+4]

; Load current (i.e. start) pointer
mov ebx, [esp+8]

.Loop:

; Load value to copy
mov ecx, [ebx]

; Load / calc pointer to copy to
mov edx, ebx
add edx, eax
mov [edx], ecx

; Shift to next dword
sub ebx, 4

; Is current pointer == end pointer
;		i.e. Is ebx == esp
cmp ebx, esp
jne .Loop

ret