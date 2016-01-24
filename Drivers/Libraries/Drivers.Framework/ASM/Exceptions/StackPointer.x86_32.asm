BITS 32

SECTION .text

GLOBAL method_System_Byte__RETEND_Drivers_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___:function
GLOBAL method_System_Void_RETEND_Drivers_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__:function

method_System_Byte__RETEND_Drivers_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___:
push dword ebp
mov dword ebp, esp

mov dword eax, esp
add eax, 12
mov dword [ebp+8], eax

pop dword ebp
ret


method_System_Void_RETEND_Drivers_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__:
mov dword eax, [esp]
mov dword esp, [esp+4]
; Handles the "add esp, 4" after return
sub esp, 4
push dword eax
ret