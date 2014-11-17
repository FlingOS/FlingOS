method_System_Void_RETEND_Kernel_Core_Processes_Scheduler_DECLEND_LoadTR_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword eax, 24  ; Selector offset of TSS - see MultibootSignature ASM file for details
ltr ax			   ; Load selector

pop dword ebp
ret



method_Kernel_Core_Processes_TSS__RETEND_Kernel_Core_Processes_Scheduler_DECLEND_GetTSSPointer_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], _NATIVE_TSS

pop dword ebp

ret