BITS 32

GLOBAL method_System_Void_RETEND_KernelABI_SystemCalls_DECLEND_Call_NAMEEND__KernelABI_SystemCalls_Calls_System_UInt32_System_UInt32_System_UInt32_System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__:function

SECTION .text
method_System_Void_RETEND_KernelABI_SystemCalls_DECLEND_Call_NAMEEND__KernelABI_SystemCalls_Calls_System_UInt32_System_UInt32_System_UInt32_System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__:
push dword ebp
mov dword ebp, esp

mov dword eax, [ebp+36]
mov dword ebx, [ebp+32]
mov dword ecx, [ebp+28]
mov dword edx, [ebp+24]
int 48
push eax
push ebx
push ecx
push edx
mov dword eax, [ebp+8]
pop dword [eax]
mov dword eax, [ebp+12]
pop dword [eax]
mov dword eax, [ebp+16]
pop dword [eax]
mov dword eax, [ebp+20]
pop dword [eax]

pop dword ebp
ret