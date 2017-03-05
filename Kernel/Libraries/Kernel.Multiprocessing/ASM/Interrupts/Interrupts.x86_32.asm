BITS 32

SECTION .text

GLOBAL method_System_Void_RETEND_Kernel_Interrupts_Interrupts_DECLEND_EnableInterrupts_NAMEEND___:function
GLOBAL method_System_Void_RETEND_Kernel_Interrupts_Interrupts_DECLEND_DisableInterrupts_NAMEEND___:function
GLOBAL method_Kernel_Interrupts_InterruptDescriptor__RETEND_Kernel_Interrupts_Interrupts_DECLEND_GetIDTPtr_NAMEEND___:function
GLOBAL method_System_Void_RETEND_Kernel_Interrupts_Interrupts_DECLEND_EndIRQ_NAMEEND__System_Boolean_:function
GLOBAL method_System_Void_RETEND_Kernel_Interrupts_Interrupts_DECLEND_InvokeInterrupt_NAMEEND__System_UInt32_:function

EXTERN IDT_Contents

method_System_Void_RETEND_Kernel_Interrupts_Interrupts_DECLEND_EnableInterrupts_NAMEEND___:
sti
nop
ret

method_System_Void_RETEND_Kernel_Interrupts_Interrupts_DECLEND_DisableInterrupts_NAMEEND___:
cli
nop
ret

method_Kernel_Interrupts_InterruptDescriptor__RETEND_Kernel_Interrupts_Interrupts_DECLEND_GetIDTPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], IDT_Contents

pop dword ebp

ret


method_System_Void_RETEND_Kernel_Interrupts_Interrupts_DECLEND_EndIRQ_NAMEEND__System_Boolean_:

push dword ebp
mov dword ebp, esp

mov byte al, [ebp+8]
test al, 0xFF
mov al, 0x20
jz .Zero
out 0xA0, al
.Zero:
out 0x20, al

pop dword ebp

ret

method_System_Void_RETEND_Kernel_Interrupts_Interrupts_DECLEND_InvokeInterrupt_NAMEEND__System_UInt32_:

push dword ebp
mov dword ebp, esp

mov eax, [ebp+8]
%macro InterruptTestMacro 1
	cmp eax, %1
	jne .Next%1
	int %1
	.Next%1:
%endmacro
%assign interrupttestnum 0
%rep 255
	InterruptTestMacro interrupttestnum
	%assign interrupttestnum interrupttestnum+1
%endrep

pop dword ebp

ret