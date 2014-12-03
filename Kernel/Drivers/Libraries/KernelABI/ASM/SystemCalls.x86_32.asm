method_System_UInt32_RETEND_KernelABI_SystemCalls_DECLEND_Call_NAMEEND__KernelABI_SystemCalls_Calls_System_UInt32_System_UInt32_System_UInt32_:
push dword ebp
mov dword ebp, esp

mov dword eax, [ebp+24]
mov dword ebx, [ebp+20]
mov dword ecx, [ebp+16]
mov dword edx, [ebp+12]
int eax
mov dword [ebp+8], eax


pop dword ebp
ret