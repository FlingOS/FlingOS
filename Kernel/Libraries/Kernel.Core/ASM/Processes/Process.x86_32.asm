method_System_UInt32_RETEND_Kernel_Core_Processes_Process_DECLEND_GetCR3_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword eax, cr3
mov dword [ebp+8], eax

pop dword ebp
ret
