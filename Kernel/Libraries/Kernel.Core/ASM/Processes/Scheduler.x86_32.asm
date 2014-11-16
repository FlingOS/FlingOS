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



method_System_Void_RETEND_Kernel_Core_Processes_Scheduler_DECLEND_JumpToMainMethod_NAMEEND__System_UInt32_System_UInt32_:

push dword ebp
mov dword ebp, esp

; Arg 2 - ESP - EBP+8
; Arg 1 - EIP - EBP+12

; Temp load eip value
mov dword eax, [ebp+12]
; Temp load esp value
mov dword ebx, [ebp+8]
; Set new esp
mov dword esp, ebx
; Jump to eip
jmp eax

Next:
mov dword esp, ebp
pop dword ebp
ret