method_System_Void__RETEND_Kernel_Kernel_DECLEND_GetMainMethodPtr_NAMEEND___:
push dword ebp
mov dword ebp, esp

mov dword [ebp+8], method_System_Void_RETEND_Kernel_Kernel_DECLEND_ManagedMain_NAMEEND___

pop dword ebp
ret

method_System_Void__RETEND_Kernel_Kernel_DECLEND_GetKernelStackPtr_NAMEEND___:
push dword ebp
mov dword ebp, esp

mov dword [ebp+8], Kernel_Stack

pop dword ebp
ret