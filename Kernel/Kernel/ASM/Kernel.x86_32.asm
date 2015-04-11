BITS 32

SECTION .text

GLOBAL method_System_Void__RETEND_Kernel_Kernel_DECLEND_GetManagedMainMethodPtr_NAMEEND___:function
GLOBAL method_System_Byte__RETEND_Kernel_Kernel_DECLEND_GetKernelStackPtr_NAMEEND___:function
GLOBAL method_System_UInt32_RETEND_Kernel_Kernel_DECLEND_GetESP_NAMEEND___:function
GLOBAL method_System_UInt32_RETEND_Kernel_Kernel_DECLEND_GetStackValue_NAMEEND__System_UInt32_:function

EXTERN method_System_Void_RETEND_Kernel_Kernel_DECLEND_ManagedMain_NAMEEND___
EXTERN Kernel_Stack

method_System_Void__RETEND_Kernel_Kernel_DECLEND_GetManagedMainMethodPtr_NAMEEND___:
push dword ebp
mov dword ebp, esp

mov dword [ebp+8], method_System_Void_RETEND_Kernel_Kernel_DECLEND_ManagedMain_NAMEEND___

pop dword ebp
ret


method_System_Byte__RETEND_Kernel_Kernel_DECLEND_GetKernelStackPtr_NAMEEND___:
push dword ebp
mov dword ebp, esp

mov dword [ebp+8], Kernel_Stack

pop dword ebp
ret



method_System_UInt32_RETEND_Kernel_Kernel_DECLEND_GetESP_NAMEEND___:
push dword ebp
mov dword ebp, esp

mov ebx, esp
add ebx, 12
mov dword [ebp+8], ebx

pop dword ebp
ret

method_System_UInt32_RETEND_Kernel_Kernel_DECLEND_GetStackValue_NAMEEND__System_UInt32_:
push dword ebp
mov dword ebp, esp

mov dword eax, [ebp+12]
mov dword ebx, esp
add ebx, 16
add ebx, eax
mov ebx, [ebx]
mov dword [ebp+8], ebx

pop dword ebp
ret