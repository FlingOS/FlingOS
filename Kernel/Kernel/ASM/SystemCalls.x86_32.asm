BITS 32

GLOBAL method_System_Void_RETEND_Kernel_Processes_SystemCalls_DECLEND_Call_NAMEEND__Kernel_Processes_SystemCallNumbers_System_UInt32_System_UInt32_System_UInt32_System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__:function

SECTION .text
method_System_Void_RETEND_Kernel_Processes_SystemCalls_DECLEND_Call_NAMEEND__Kernel_Processes_SystemCallNumbers_System_UInt32_System_UInt32_System_UInt32_System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__System_UInt32_AMP__:
push dword ebp
mov dword ebp, esp

; Load arguments into argument registers
mov dword eax, [ebp+36]
mov dword ebx, [ebp+32]
mov dword ecx, [ebp+28]
mov dword edx, [ebp+24]
; Do the system call
int 48
; Push return values
push eax
push ebx
push ecx
push edx
; Store return values
;	Note: Arguments are references (pointers) to integers
;		  So the value of the arguments are addresses
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